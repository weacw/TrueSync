﻿using UnityEngine;

namespace TrueSync {

    /**
    *  @brief A deterministic version of Unity's Transform component for 3D physics. 
    **/
    [ExecuteInEditMode]
    public class TSTransform : MonoBehaviour {

        private const float DELTA_TIME_FACTOR = 10f;

        [SerializeField]
        [HideInInspector]
        [AddTracking]
        private TSVector _position;

        /**
        *  @brief Property access to position. 
        *  
        *  It works as proxy to a Body's position when there is a collider attached.
        **/
        public TSVector position {
            get {
                if (tsCollider != null && tsCollider.Body != null) {
					return tsCollider.Body.TSPosition - scaledCenter;
                }

				return _position;
            }
            set {
                _position = value;

                if (tsCollider != null && tsCollider.Body != null) {
                    tsCollider.Body.TSPosition = _position + scaledCenter;
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        [AddTracking]
        private TSQuaternion _rotation;

        /**
        *  @brief Property access to rotation. 
        *  
        *  It works as proxy to a Body's rotation when there is a collider attached.
        **/
        public TSQuaternion rotation {
            get {
                if (tsCollider != null && tsCollider.Body != null) {
                    return TSQuaternion.CreateFromMatrix(tsCollider.Body.TSOrientation);
                }

                return _rotation;
            }
            set {
                _rotation = value;

                if (tsCollider != null && tsCollider.Body != null) {
                    tsCollider.Body.TSOrientation = TSMatrix.CreateFromQuaternion(_rotation);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        [AddTracking]
        private TSVector _scale;

        /**
        *  @brief Property access to scale. 
        **/
        public TSVector scale {
            get {
                return _scale;
            }
            set {
                _scale = value;
            }
        }

        [SerializeField]
        [HideInInspector]
        private bool _serialized;

        private TSVector scaledCenter {
            get {
                if (tsCollider != null) {
                    return tsCollider.ScaledCenter;
                }

                return TSVector.zero;
            }
        }

        /**
        *  @brief Rotates game object to point forward vector to a target position. 
        *  
        *  @param other TSTrasform used to get target position.
        **/
        public void LookAt(TSTransform other) {
            LookAt(other.position);
        }

        /**
        *  @brief Rotates game object to point forward vector to a target position. 
        *  
        *  @param target Target position.
        **/
        public void LookAt(TSVector target) {
            this.rotation = TSQuaternion.CreateFromMatrix(TSMatrix.CreateFromLookAt(position, target));
        }

        /**
        *  @brief Moves game object based on provided axis values. 
        **/
        public void Translate(FP x, FP y, FP z) {
            Translate(x, y, z, Space.Self);
        }

        /**
        *  @brief Moves game object based on provided axis values and a relative space.
        *  
        *  If relative space is SELF then the game object will move based on its forward vector.
        **/
        public void Translate(FP x, FP y, FP z, Space relativeTo) {
            Translate(new TSVector(x, y, z), relativeTo);
        }

        /**
        *  @brief Moves game object based on provided axis values and a relative {@link TSTransform}.
        *  
        *  The game object will move based on TSTransform's forward vector.
        **/
        public void Translate(FP x, FP y, FP z, TSTransform relativeTo) {
            Translate(new TSVector(x, y, z), relativeTo);
        }

        /**
        *  @brief Moves game object based on provided translation vector.
        **/
        public void Translate(TSVector translation) {
            Translate(translation, Space.Self);
        }

        /**
        *  @brief Moves game object based on provided translation vector and a relative space.
        *  
        *  If relative space is SELF then the game object will move based on its forward vector.
        **/
        public void Translate(TSVector translation, Space relativeTo) {
            if (relativeTo == Space.Self) {
                Translate(translation, this);
            } else {
                this.position += translation;
            }
        }

        /**
        *  @brief Moves game object based on provided translation vector and a relative {@link TSTransform}.
        *  
        *  The game object will move based on TSTransform's forward vector.
        **/
        public void Translate(TSVector translation, TSTransform relativeTo) {
            this.position += TSVector.Transform(translation, TSMatrix.CreateFromQuaternion(relativeTo.rotation));
        }

        /**
        *  @brief Rotates game object based on provided axis, point and angle of rotation.
        **/
        public void RotateAround(TSVector point, TSVector axis, FP angle) {
            TSVector vector = this.position;
            TSVector vector2 = vector - point;
            vector2 = TSVector.Transform(vector2, TSMatrix.AngleAxis(angle * FP.Deg2Rad, axis));
            vector = point + vector2;
            this.position = vector;

            Rotate(axis, angle);
        }

        /**
        *  @brief Rotates game object based on provided axis and angle of rotation.
        **/
        public void RotateAround(TSVector axis, FP angle) {
            Rotate(axis, angle);
        }

        /**
        *  @brief Rotates game object based on provided axis angles of rotation.
        **/
        public void Rotate(FP xAngle, FP yAngle, FP zAngle) {
            Rotate(new TSVector(xAngle, yAngle, zAngle), Space.Self);
        }

        /**
        *  @brief Rotates game object based on provided axis angles of rotation and a relative space.
        *  
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
        public void Rotate(FP xAngle, FP yAngle, FP zAngle, Space relativeTo) {
            Rotate(new TSVector(xAngle, yAngle, zAngle), relativeTo);
        }

        /**
        *  @brief Rotates game object based on provided axis angles of rotation.
        **/
        public void Rotate(TSVector eulerAngles) {
            Rotate(eulerAngles, Space.Self);
        }

        /**
        *  @brief Rotates game object based on provided axis and angle of rotation.
        **/
        public void Rotate(TSVector axis, FP angle) {
            Rotate(axis, angle, Space.Self);
        }

        /**
        *  @brief Rotates game object based on provided axis, angle of rotation and relative space.
        *  
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
        public void Rotate(TSVector axis, FP angle, Space relativeTo) {
            TSQuaternion result = TSQuaternion.identity;

            if (relativeTo == Space.Self) {
                result = this.rotation * TSQuaternion.AngleAxis(angle, axis);
            } else {
                result = TSQuaternion.AngleAxis(angle, axis) * this.rotation;
            }

            result.Normalize();
            this.rotation = result;
        }

        /**
        *  @brief Rotates game object based on provided axis angles and relative space.
        *  
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
        public void Rotate(TSVector eulerAngles, Space relativeTo) {
            TSQuaternion result = TSQuaternion.identity;

            if (relativeTo == Space.Self) {
                result = this.rotation * TSQuaternion.Euler(eulerAngles);
            } else {
                result = TSQuaternion.Euler(eulerAngles) * this.rotation;
            }

            result.Normalize();
            this.rotation = result;
        }

        /**
        *  @brief Current self forward vector.
        **/
        public TSVector forward {
            get {
                return TSVector.Transform(TSVector.forward, TSMatrix.CreateFromQuaternion(rotation));
            }
        }

        /**
        *  @brief Current self right vector.
        **/
        public TSVector right {
            get {
                return TSVector.Transform(TSVector.right, TSMatrix.CreateFromQuaternion(rotation));
            }
        }

        /**
        *  @brief Current self up vector.
        **/
        public TSVector up {
            get {
                return TSVector.Transform(TSVector.up, TSMatrix.CreateFromQuaternion(rotation));
            }
        }

        /**
        *  @brief Returns Euler angles in degrees.
        **/
        public TSVector eulerAngles {
            get {
                return rotation.eulerAngles;
            }
        }


        public TSMatrix LocalToWorldMatrix
        {
            get
            {
                TSMatrix curMatrix = TSMatrix.CreateFromQuaternion(rotation);
                TSTransform curParent = tsParent;
                while (curParent != null)
                {
                    curMatrix *= TSMatrix.CreateFromQuaternion(curParent.rotation);
                    curParent = curParent.tsParent;
                }
                return curMatrix;
            }
        }

        public TSMatrix WorldToLocalMatrix
        {
            get
            {
                return TSMatrix.Inverse(LocalToWorldMatrix);
            }
        }

        /**
         *  @brief Transform a point from local space to world space.
         **/
        public TSVector TransformPoint(TSVector point)
        {
            TSVector result = TSVector.Transform(point, TSMatrix.CreateFromQuaternion(rotation));
            result.x *= scale.x;
            result.y *= scale.y;
            result.z *= scale.z;
            return result + this.position;
        }

        /**
         *  @brief Transform a point from world space to local space.
         **/
        public TSVector InverseTransformPoint(TSVector point)
        {
            point -= this.position;
            point.x /= scale.x;
            point.y /= scale.y;
            point.z /= scale.z;
            TSMatrix m = TSMatrix.CreateFromQuaternion(rotation);
            m = TSMatrix.Inverse(m);
            TSVector result = TSVector.Transform(point, m);
            return result;
        }

        /**
         *  @brief Transform a direction from local space to world space.
         **/
        public TSVector TransformDirection(TSVector direction)
        {
            return TSVector.Transform(direction, TSMatrix.CreateFromQuaternion(rotation));
        }

        /**
         *  @brief Transform a direction from world space to local space.
         **/
        public TSVector InverseTransformDirection(TSVector direction)
        {
            TSMatrix m = TSMatrix.CreateFromQuaternion(rotation);
            m = TSMatrix.Inverse(m);
            return TSVector.Transform(direction, m);
        }

        /**
         *  @brief Transform a vector from local space to world space.
         **/
        public TSVector TransformVector(TSVector vector)
        {
            TSVector result = TSVector.Transform(vector, TSMatrix.CreateFromQuaternion(rotation));
            result.x *= scale.x;
            result.y *= scale.y;
            result.z *= scale.z;
            return result;
        }

        /**
         *  @brief Transform a vector from world space to local space.
         **/
        public TSVector InverseTransformVector(TSVector vector)
        {
            vector.x /= scale.x;
            vector.y /= scale.y;
            vector.z /= scale.z;
            TSMatrix m = TSMatrix.CreateFromQuaternion(rotation);
            m = TSMatrix.Inverse(m);
            TSVector result = TSVector.Transform(vector, m);
            return result;
        }

        [HideInInspector]
        public TSCollider tsCollider;

        [HideInInspector]
        public TSTransform tsParent;

        private bool initialized = false;

		private TSRigidBody rb;

        public void Start() {
            if (!Application.isPlaying) {
                return;
            }

            Initialize();
			rb = GetComponent<TSRigidBody> ();
        }

        /**
        *  @brief Initializes internal properties based on whether there is a {@link TSCollider} attached.
        **/
        public void Initialize() {
            if (initialized) {
                return;
            }

            tsCollider = GetComponent<TSCollider>();
            if (transform.parent != null) {
                tsParent = transform.parent.GetComponent<TSTransform>();
            }

            if (!_serialized) {
                UpdateEditMode();
            }

            if (tsCollider != null) {
                if (tsCollider.IsBodyInitialized) {
                    tsCollider.Body.TSPosition = _position + scaledCenter;
                    tsCollider.Body.TSOrientation = TSMatrix.CreateFromQuaternion(_rotation);
                }
            } else {
                StateTracker.AddTracking(this);
            }

            initialized = true;
        }

        public void Update() {
            if (Application.isPlaying) {
                if (initialized) {
                    UpdatePlayMode();
                }
            } else {
                UpdateEditMode();
            }
        }

        private void UpdateEditMode() {
            if (transform.hasChanged) {
                _position = transform.position.ToTSVector();
                _rotation = transform.rotation.ToTSQuaternion();
                _scale = transform.localScale.ToTSVector();

                _serialized = true;
            }
        }

        private void UpdatePlayMode() {
			if (rb != null) {
                if (rb.interpolation == TSRigidBody.InterpolateMode.Interpolate) {
                    transform.position = Vector3.Lerp(transform.position, position.ToVector(), Time.deltaTime * DELTA_TIME_FACTOR);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation.ToQuaternion(), Time.deltaTime * DELTA_TIME_FACTOR);
                    transform.localScale = Vector3.Lerp(transform.localScale, scale.ToVector(), Time.deltaTime * DELTA_TIME_FACTOR);
                    return;
                } else if (rb.interpolation == TSRigidBody.InterpolateMode.Extrapolate) {
                    transform.position = (position + rb.tsCollider.Body.TSLinearVelocity * Time.deltaTime * DELTA_TIME_FACTOR).ToVector();
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation.ToQuaternion(), Time.deltaTime * DELTA_TIME_FACTOR);
                    transform.localScale = Vector3.Lerp(transform.localScale, scale.ToVector(), Time.deltaTime * DELTA_TIME_FACTOR);
                    return;
                }
			}

            transform.position = position.ToVector();
            transform.rotation = rotation.ToQuaternion();
            transform.localScale = scale.ToVector();
        }

    }

}