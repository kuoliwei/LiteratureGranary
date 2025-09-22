using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTypes
{
    /// <summary>���[���`���ޡ]0~16�^�A�����A���Ѫ��R�W�C</summary>
    public enum JointId
    {
        Nose = 0,
        LeftEye = 1,
        RightEye = 2,
        LeftEar = 3,
        RightEar = 4,
        LeftShoulder = 5,
        RightShoulder = 6,
        LeftElbow = 7,
        RightElbow = 8,
        LeftWrist = 9,
        RightWrist = 10,
        LeftHip = 11,
        RightHip = 12,
        LeftKnee = 13,
        RightKnee = 14,
        LeftAnkle = 15,
        RightAnkle = 16,
    }

    public static class PoseSchema
    {
        public const int JointCount = 17;
    }

    /// <summary>
    /// ��@���`��ơGx, y, z, conf�]�H��/�i���ס^�C
    /// ��ĳ conf > 0 �������ġ]����ѪR���|�̦��]�m�^�C
    /// </summary>
    [Serializable]
    public struct Joint
    {
        public float x;
        public float y;
        public float z;
        public float conf;

        public Joint(float x, float y, float z, float conf)
        {
            this.x = x; this.y = y; this.z = z; this.conf = conf;
        }

        /// <summary>�O�_�i�Ρ]����ѪR�ɥi�� conf<=0 �N��L�ġ^�C</summary>
        public bool IsValid => conf > 0f;

        /// <summary>��K���Ϊ� XYZ�C</summary>
        public Vector3 XYZ => new Vector3(x, y, z);

        public override string ToString() => $"({x:F3},{y:F3},{z:F3}|c={conf:F2})";
    }

    /// <summary>
    /// ��@�H�����@��� 17 �����`�C
    /// �ϥ� class �H�קK�ȫ��O�����y�����}�C�ޥβV�c�P�įন���C
    /// </summary>
    [Serializable]
    public class PersonSkeleton
    {
        // �T�w���� 17
        public Joint[] joints = new Joint[PoseSchema.JointCount];

        /// <summary>�H JointId ����/�]�w���`�C</summary>
        public Joint this[JointId id]
        {
            get => joints[(int)id];
            set => joints[(int)id] = value;
        }

        /// <summary>�w��Ū���]�Y��Ƥ����B���޿��~�ɤ��ߨҥ~�^�C</summary>
        public bool TryGet(JointId id, out Joint j)
        {
            int idx = (int)id;
            if (joints != null && idx >= 0 && idx < joints.Length)
            {
                j = joints[idx];
                return true;
            }
            j = default;
            return false;
        }
    }

    /// <summary>
    /// �@�Ӽv�檺�h�H���[��ơC
    /// �A�� Server �T���O {"<frameIndex>": [ persons... ]}�A
    /// �ѪR��ڭ̲Τ@�� int frameIndex + List<PersonSkeleton> ��ܡC
    /// </summary>
    [Serializable]
    public class FrameSample
    {
        public int frameIndex;
        public List<PersonSkeleton> persons = new List<PersonSkeleton>();

        public FrameSample() { }
        public FrameSample(int frameIndex) { this.frameIndex = frameIndex; }
    }
}
