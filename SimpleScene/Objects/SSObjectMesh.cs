// Copyright(C) David W. Jeske, 2013
// Released to the public domain. Use, modify and relicense at will.

using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SimpleScene
{
    public class SSObjectMesh : SSObject
    {
		public SSObjectMesh () { 
            this.renderState.castsShadow = true;    // SSObjectMesh casts shadow by default
            this.renderState.receivesShadows = true; // SSObjectMesh receives shadow by default
        }        
        public SSObjectMesh (SSAbstractMesh mesh) : this() {
            this.Mesh = mesh;        
            this.renderState.castsShadow = true;    // SSObjectMesh casts shadow by default
            this.renderState.receivesShadows = true; // SSObjectMesh receives shadow by default
			_setupMesh ();
        }
		
        private SSAbstractMesh _mesh;
        public SSAbstractMesh Mesh {
          get { return _mesh; }
          set { _mesh = value; _setupMesh(); }
        }

		public override bool alphaBlendingEnabled {
			get {
				return base.alphaBlendingEnabled || _mesh.alphaBlendingEnabled;
			}
			set {
				base.alphaBlendingEnabled = value;
			}
		}

		public override Vector3 localBoundingSphereCenter {
			get {
				return _mesh.boundingSphereCenter;
			}
		}

		public override float localBoundingSphereRadius {
			get {
				return _mesh.boundingSphereRadius;
			}
		}
        
        public override void Render (SSRenderConfig renderConfig)
		{
			if (_mesh != null) {
				base.Render (renderConfig);
				this._mesh.renderMesh (renderConfig);
            }
        }

        private void _setupMesh() {
            if (_mesh != null) {
                // compute and setup bounding sphere

				// TODO: make a more detailed collision mesh

				// notify listeners..
				NotifyPositionOrSizeChanged(); 
			} 
        }

		protected override bool PreciseIntersect (ref SSRay worldSpaceRay, out float distanceAlongWorldRay)
		{
            SSRay localRay = worldSpaceRay.Transformed (this.worldMat.Inverted ());
            if (this.Mesh != null) {
                float localNearestContact;
                bool ret = this.Mesh.preciseIntersect(ref localRay, out localNearestContact);
                if (ret) {
                    Vector3 localContactPt = localRay.pos + localNearestContact * localRay.dir;
                    Vector3 worldContactPt = Vector3.Transform(localContactPt, this.worldMat);
                    distanceAlongWorldRay = (worldContactPt - worldSpaceRay.pos).Length;
                    //Console.WriteLine ("Nearest Triangle Hit @ {0} vs Sphere {1} : Object {2}", worldSpaceContactDistance, distanceAlongRay, Name);
                } else {
                    distanceAlongWorldRay = float.PositiveInfinity;
                }
                return ret;
            } else {
                distanceAlongWorldRay = float.PositiveInfinity;
                return false;
            }
		}

		public override void Update(float elapsedS) 
		{
			if (Mesh.updateSource.Target == null) {
				Mesh.updateSource.Target = this;
			}
			if (Mesh.updateSource.Target == this) {
				Mesh.update (elapsedS);
			}
		}

		private void MeshPositionOrSizeChanged() {
			NotifyPositionOrSizeChanged ();
		}
    }
}

