using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace MyParticleSystem
{
	public enum ParticleEnum
	{
			NUM_1K = 1024,
			NUM_2K = 1024 * 2,
			NUM_4K = 1024 * 4,
			NUM_8K = 1024 * 8
	}

	public struct ParticleData
	{
		public Vector3 pos;
		public Vector3 vel;
	};
	
	public class ParticleBase : MonoBehaviour
	{
		#region Value
		[SerializeField] private ParticleEnum EnumparticleNum = ParticleEnum.NUM_8K;
		private int numPrtilces;
		#endregion

		#region ComputeShader
		[SerializeField]private ComputeShader ParticleCS;
		private int THREAD_SIZE_X = 1024;

		private ComputeBuffer particleBufferRead;
		private ComputeBuffer particleBufferWrite;
		#endregion

		#region property
		public int NumParticles
		{
			get { return numPrtilces; }
		}


		public ComputeBuffer ParticleBufferRead
		{
			get { return particleBufferRead; }
		}

		#endregion
		
		#region mono
		void Awake()
		{
			numPrtilces = (int) EnumparticleNum*100;
			Debug.Log(numPrtilces);
			ParticleCS = (ComputeShader)Resources.Load("Particle");
		}
		
		void Start()
		{
			InitBuffers();
		}

		void Update()
		{
			//shaderへ転送
			ParticleCS.SetInt("_NumParticles", NumParticles);
			MainRoutine();
		}

		private void OnDestroy()
		{
			DeleteBuffer(particleBufferRead);
			DeleteBuffer(particleBufferWrite);
		}

		void InitBuffers()
		{
			//bufferのインスタンス化
			particleBufferRead = new ComputeBuffer(NumParticles, Marshal.SizeOf(typeof(ParticleData)));
			particleBufferWrite = new ComputeBuffer(NumParticles, Marshal.SizeOf(typeof(ParticleData)));
			//buffer内部の初期化
			var particles = new ParticleData[NumParticles];
			initParticleData(ref particles);
			particleBufferRead.SetData(particles);
			
			//DebugSuccess
//			foreach (var p in particles)
//			{
//				Debug.Log(p.pos);
//				Debug.Log(p.vel);
//			}
//			
			particles = null;
	}

		void initParticleData(ref ParticleData[] particles)
		{
			//Debug.Log("initParticleData" + particles.Length);
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].pos = Random.insideUnitSphere * 3.0f;
				particles[i].vel = new Vector3(0.0f, 0.0f, 0.0f);
			}
		}

		void MainRoutine()
		{
			int KernelID = -1;
			int threadGroupsX = NumParticles / THREAD_SIZE_X;

			KernelID = ParticleCS.FindKernel("Update");
			ParticleCS.SetBuffer(KernelID,"_ParticleBufferRead",particleBufferRead);
			ParticleCS.SetBuffer(KernelID,"_ParticleBufferWrite",particleBufferWrite);
			ParticleCS.Dispatch(KernelID, threadGroupsX, 1, 1);
			SwapComputeBuffer(ref particleBufferRead, ref particleBufferWrite);
		}

		private void DeleteBuffer(ComputeBuffer buffer)
		{
			if (buffer != null)
			{
				buffer.Release();
				buffer = null;
			}
		}
		
		
		private void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong) {
			ComputeBuffer temp = ping;
			ping = pong;
			pong = temp;
		}
		#endregion
	}
}
