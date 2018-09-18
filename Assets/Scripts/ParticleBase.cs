using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;




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
		private float mass = 0.1f;
		private float AttractMass = 0.5f;
		private float pi = 3.14f;
		#endregion

		#region ComputeShader
		[SerializeField]private ComputeShader ParticleCS;
		private int THREAD_SIZE_X = 1024;

		private ComputeBuffer particleBufferRead;
		private ComputeBuffer particleBufferWrite;
		private ComputeBuffer normalBufferRead;
		private ComputeBuffer particleDis;
		//private ComputeBuffer particleForceBufferRead;
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
			numPrtilces = (int) EnumparticleNum * 100;
			ParticleCS = (ComputeShader)Resources.Load("Particle");
		}
		
		void Start()
		{
			InitBuffers();
			Debug.Log(Mathf.PI);

		}

		void Update()
		{
			//shaderへ転送
			ParticleCS.SetInt("_NumParticles", NumParticles);
			ParticleCS.SetFloat("_Time", Mathf.Deg2Rad * Time.realtimeSinceStartup);
			ParticleCS.SetFloat("_Mass", mass);
			ParticleCS.SetFloat("_AttractMass", AttractMass);
			ParticleCS.SetVector("_AttPos", new Vector3(0.0f, 0.0f, 0.0f));
			
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
			normalBufferRead = new ComputeBuffer(NumParticles, Marshal.SizeOf(typeof(Vector3)));
			particleDis = new ComputeBuffer(NumParticles, Marshal.SizeOf(typeof(float)));
			//particleForceBufferRead = new ComputeBuffer(NumParticles, Marshal.SizeOf(typeof(Vector3)));
			//buffer内部の初期化
			var particles = new ParticleData[NumParticles];
			initParticleData(ref particles);
			particleBufferRead.SetData(particles);
			/*
			//----normal
			var normals = new Vector3[NumParticles];
			//----dis
			var dis = new float[NumParticles];
			initInsideSphereData(ref particles, ref normals, ref dis);
			particleBufferRead.SetData(particles);
			normalBufferRead.SetData(normals);
			particleDis.SetData(dis);
			*/
			
			particles = null;
	}

		void initParticleData(ref ParticleData[] particles)
		{
			//Debug.Log("initParticleData" + particles.Length);
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].pos = Random.insideUnitSphere * 5.0f;
				//particles[i].pos = new Vector3(0.0f, 0.0f, 100.0f);
				particles[i].vel = new Vector3(0.0f, 0.0f, 0.0f);
			}
		}


		void initInsideSphereData(ref ParticleData[] particles, ref Vector3[] normals, ref float[] dis)
		{
			/*
			theta//0<=theta<=2pi
			phi//0<=phi<pi
			x=rsinθcosϕ 
			y=rsinθsinϕ
			z=rcosθ
			*/
			float radius = 3.0f;
			int index = 0;
			float step = 0.01f;
			Vector3 cecnter = new Vector3(0.0f, 0.0f, 0.0f);
			
			for (float theta = 0.0f; theta <= 2 * pi; theta += step)
			{
				for (float phi = 0.0f; phi < pi; phi += step)
				{
					particles[index].pos.x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
					particles[index].pos.y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
					particles[index].pos.z = radius * Mathf.Cos(theta);
					normals[index] = (cecnter - particles[index].pos).normalized;
					dis[index] = (cecnter - particles[index].pos).magnitude;
					if (index > numPrtilces)
					{
						break;
					}
					index++;
				}
			}
		}

		void MainRoutine()
		{
			int KernelID = -1;
			int threadGroupsX = NumParticles / THREAD_SIZE_X;

			KernelID = ParticleCS.FindKernel("Update");
			ParticleCS.SetBuffer(KernelID,"_ParticleBufferRead",particleBufferRead);
			ParticleCS.SetBuffer(KernelID,"_ParticleBufferWrite",particleBufferWrite);
			ParticleCS.SetBuffer(KernelID, "_NormalBufferRead", normalBufferRead);
			ParticleCS.SetBuffer(KernelID, "_ParticleDis", particleDis);
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
