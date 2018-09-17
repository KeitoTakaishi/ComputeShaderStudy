using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyParticleSystem
{
	public class RendererScript : MonoBehaviour
	{
		
		[SerializeField] private Material _RenderMat;
		[SerializeField] private Shader _shader;
		public ParticleBase particleBase;
		private int modelMatrixPropId;
		
		#region property
		public Material material
		{
			get { return _RenderMat; }
		}		
		public Shader shader
		{
			get { return _shader; }
		}
		#endregion

		void Start()
		{
			modelMatrixPropId = Shader.PropertyToID("modelMatrix");
		}

		
		private void OnRenderObject()
		{
			_RenderMat.SetPass(0);
			//buffer内部の情報をVertexShaderに転送
			_RenderMat.SetBuffer("_ParticlesBuffer", particleBase.ParticleBufferRead);
			Matrix4x4 modelMatrix = transform.localToWorldMatrix;
			_RenderMat.SetMatrix(modelMatrixPropId, modelMatrix);
			Graphics.DrawProcedural(MeshTopology.Points, particleBase.NumParticles);
		}
	}
}