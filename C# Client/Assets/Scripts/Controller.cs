using System;
using Grpc.Core;
using Subs;

namespace QubeView
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    public class Controller : MonoBehaviour
    {
        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
        
        private Channel _channel;
        private SubGetResponse _response;
        private SubsService.SubsServiceClient _client;
        
        private bool _mTargetFound;
        private Anchor _anchorPlace;

        private Pose _pose;
        private Pose _worldPose;

        private float _anchorPositionX;
        private float _anchorPositionY;
        private float _anchorPositionZ;
        
        private Quaternion _anchorRotation;
        private float _anchorRotationW;
        private float _anchorRotationX;
        private float _anchorRotationY;
        private float _anchorRotationZ;

        public Subtitle subtitle;
        public Transform arCoreDeviceTransform;
        public SubInitialCheckResponse Initialcheck;

        public void Start()
        {
            _channel = new Channel("192.168.43.1:33455", ChannelCredentials.Insecure);
            
            _client = new SubsService.SubsServiceClient(_channel);
            
            Initialcheck = _client.SubInitialCheck(new SubInitialCheckRequest());

            if (Initialcheck.IsSet)
            {
                _response = _client.SubGet(new SubGetRequest());
            }
        }
        
        public void Awake()
        {
            Application.targetFrameRate = 60;
        }

        public void Update()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (_mTargetFound == false)
            {
                if (Initialcheck.IsSet == false)
                {
                    // Get updated augmented images for this frame.
                    Session.GetTrackables(m_TempAugmentedImages, TrackableQueryFilter.Updated);

                    foreach (var image in m_TempAugmentedImages)
                    {
                        if (image.TrackingState == TrackingState.Tracking)
                        {
                            subtitle.transform.localScale =
                                new Vector3(Convert.ToSingle(0.0003), Convert.ToSingle(0.0003), 1);

                            _anchorPositionX = image.CenterPose.position.x - 0.16f;
                            _anchorPositionY = image.CenterPose.position.y - 0.1f;
                            _anchorPositionZ = image.CenterPose.position.z;

                            _anchorRotation = Quaternion.Euler(0, 0, 0);
                            _anchorRotationX = _anchorRotation.x;
                            _anchorRotationY = _anchorRotation.y;
                            _anchorRotationZ = _anchorRotation.z;
                            _anchorRotationW = _anchorRotation.w;
                            
                            _pose = new Pose(new Vector3(_anchorPositionX, _anchorPositionY, _anchorPositionZ), new 
                            Quaternion(_anchorRotationX, _anchorRotationY, _anchorRotationZ, _anchorRotationW));

                            _anchorPlace = Session.CreateAnchor(_pose);
                            
                            _worldPose = SetWorldOrigin(_anchorPlace.transform);

                            arCoreDeviceTransform.SetPositionAndRotation(_worldPose.position, _worldPose.rotation);
                            
                            _client.SubSet(new SubSetRequest
                            {
                                AnchorPositionX = _anchorPositionX,
                                AnchorPositionY = _anchorPositionY,
                                AnchorPositionZ = _anchorPositionZ,
                                AnchorRotationW = _anchorRotationW,
                                AnchorRotationX = _anchorRotationX,
                                AnchorRotationY = _anchorRotationY,
                                AnchorRotationZ = _anchorRotationZ,
                            });
                            
                            Instantiate(subtitle, Vector3.zero, Quaternion.identity);
                            
                            _mTargetFound = true;

                            return;
                        }
                    }
                }
                else
                {
                    subtitle.transform.localScale =
                        new Vector3(Convert.ToSingle(0.0004), Convert.ToSingle(0.0004), 1);
                    
                    _pose = new Pose(new Vector3(_response.AnchorPositionX, _response.AnchorPositionY,
                        _response.AnchorPositionZ), new Quaternion(_response.AnchorRotationX, _response
                        .AnchorRotationY, _response.AnchorRotationZ, _response.AnchorRotationW));
                    
                    _anchorPlace = Session.CreateAnchor(_pose);

                    _worldPose = SetWorldOrigin(_anchorPlace.transform);
                    
                    arCoreDeviceTransform.SetPositionAndRotation(_worldPose.position, _worldPose.rotation);

                    Instantiate(subtitle, Vector3.zero, Quaternion.identity);

                    _mTargetFound = true;
                }
            }
            
        }
        private Pose SetWorldOrigin(Transform anchorTransform)
        {
            
            Pose worldPose = _WorldToAnchorPose(new Pose(arCoreDeviceTransform.position,
                arCoreDeviceTransform.rotation), anchorTransform);
            
            return worldPose;

        }
        
        private Pose _WorldToAnchorPose(Pose pose, Transform anchorTransform)
        {
            
            Matrix4x4 anchorTWorld = Matrix4x4.TRS(
                anchorTransform.position, anchorTransform.rotation, Vector3.one).inverse;

            Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
            
            Quaternion rotation = pose.rotation * Quaternion.LookRotation(
                                      anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

            return new Pose(position, rotation);
        }
    }
}
