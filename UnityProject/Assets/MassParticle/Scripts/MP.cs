﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Explicit)]
public struct MPParticle
{
	[FieldOffset(0)]  public Vector4 position;	// union
	[FieldOffset(0)]  public Vector3 position3; // 
	[FieldOffset(16)] public Vector4 velocity;	// union
	[FieldOffset(16)] public Vector3 velocity3;	// 
	[FieldOffset(28)] public float speed;		// 
	[FieldOffset(32)] public float density;
	[FieldOffset(36)] public uint hash;		// union
	[FieldOffset(36)] public int hit_prev;	// 
	[FieldOffset(40)] public int hit;
	[FieldOffset(44)] public float lifetime;
};

[StructLayout(LayoutKind.Explicit)]
public struct MPKernelParams
{
	[FieldOffset(0)]   public Vector3 WorldCenter;
	[FieldOffset(16)]  public Vector3 WorldSize;
	[FieldOffset(32)]  public int WorldDiv_x;
	[FieldOffset(36)]  public int WorldDiv_y;
	[FieldOffset(40)]  public int WorldDiv_z;
	[FieldOffset(48)]  public Vector3 Scaler;
	[FieldOffset(64)]  public int SolverType;
	[FieldOffset(68)]  public float LifeTime;
	[FieldOffset(72)]  public float Timestep;
	[FieldOffset(76)]  public float Decelerate;
	[FieldOffset(80)]  public float PressureStiffness;
	[FieldOffset(84)]  public float WallStiffness;
	[FieldOffset(88)]  public int MaxParticles;
	[FieldOffset(92)]  public float ParticleSize;
	[FieldOffset(96)]  public float SPHRestDensity;
	[FieldOffset(100)] public float SPHParticleMass;
	[FieldOffset(104)] public float SPHViscosity;
};

public enum MPSolverType
{
	Impulse = 0,
	SPH = 1,
	SPHEstimate = 2,
	NoInteraction = 3,
}

public struct MPColliderProperties
{
	public uint group_mask;
	public int owner_id;
	public float stiffness;
	public float bounce;
	public float damage_on_hit;

	public void SetDefaultValues()
	{
		group_mask = 0xffffffff;
		owner_id = -1;
		stiffness = 1500.0f;
		bounce = 1.0f;
		damage_on_hit = 0.0f;
	}
}


public enum MPForceShape
{
	All,
	Sphere,
	Capsule,
	Box
}

public enum MPForceDirection
{
	Directional,
	Radial,
	RadialCapsule,
	Vortex,
	Spline,
	VectorField,
}

public struct MPForceProperties
{
	public uint group_mask;
	public MPForceShape shape_type;
	public MPForceDirection dir_type;
	public float strength_near;
	public float strength_far;
	public float range_inner;
	public float range_outer;
	public float attenuation_exp;

	public Vector3 directional_pos;
	public Vector3 directional_dir;
	public Vector3 radial_center;
	public Vector3 vortex_pos;
	public Vector3 vortex_axis;
	public float vortex_pull;

	public void SetDefaultValues()
	{
		group_mask = 0xffffffff;
		attenuation_exp = 0.25f;
	}
}

public unsafe struct MPMeshData
{
	public int* indices;
	public Vector3* vertices;
	public Vector3* normals;
	public Vector2* uv;
};

public class MPAPI {
	
	[DllImport ("MassParticleHelper")] unsafe public static extern void mphInitialize();

	[DllImport ("MassParticle")] unsafe public static extern void mpGeneratePointMesh(int i, ref MPMeshData md);
	[DllImport ("MassParticle")] unsafe public static extern void mpGenerateCubeMesh(int i, ref MPMeshData md);
	[DllImport ("MassParticle")] unsafe public static extern void mpUpdateDataTexture(IntPtr tex);
	[DllImport ("MassParticle")] public static extern void mpReloadShader ();

	[DllImport ("MassParticle")] public static extern void mpUpdate (float dt);
	[DllImport ("MassParticle")] public static extern void mpClearParticles();
	[DllImport ("MassParticle")] public static extern void mpClearCollidersAndForces();
	[DllImport ("MassParticle")] public static extern void mpSetViewProjectionMatrix(Matrix4x4 view, Matrix4x4 proj, Vector3 cameraPos);
	[DllImport ("MassParticle")] public static extern MPKernelParams mpGetKernelParams();
	[DllImport ("MassParticle")] public static extern void mpSetKernelParams(ref MPKernelParams p);

	[DllImport ("MassParticle")] public static extern int mpGetNumParticles();
	[DllImport ("MassParticle")] unsafe public static extern MPParticle* mpGetParticles();
	[DllImport ("MassParticle")] unsafe public static extern void mpCopyParticles (MPParticle *dst);
	[DllImport ("MassParticle")] unsafe public static extern void mpWriteParticles(MPParticle *from);
	[DllImport ("MassParticle")] public static extern void mpScatterParticlesSphere(Vector3 center, float radius, int num, Vector3 velBase, float velDiffuse);
	[DllImport ("MassParticle")] public static extern void mpScatterParticlesBox(Vector3 center, Vector3 size, int num, Vector3 velBase, float velDiffuse);
	[DllImport ("MassParticle")] public static extern void mpScatterParticlesSphereTransform(Matrix4x4 trans, int num, Vector3 velBase, float velDiffuse);
	[DllImport ("MassParticle")] public static extern void mpScatterParticlesBoxTransform(Matrix4x4 trans, int num, Vector3 velBase, float velDiffuse);

	[DllImport ("MassParticle")] public static extern void mpAddSphereCollider(ref MPColliderProperties props, Vector3 center, float radius);
	[DllImport ("MassParticle")] public static extern void mpAddCapsuleCollider(ref MPColliderProperties props, Vector3 pos1, Vector3 pos2, float radius);
	[DllImport ("MassParticle")] public static extern void mpAddBoxCollider(ref MPColliderProperties props, Matrix4x4 transform, Vector3 size);
	[DllImport ("MassParticle")] public static extern void mpAddForce (ref MPForceProperties regionType, Matrix4x4 regionMat);

}


public class MPUtils
{
	public static void AddRadialSphereForce(Vector3 pos, float radius, float strength)
	{
		Matrix4x4 mat = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * radius);
		MPForceProperties p = new MPForceProperties();
		p.shape_type = MPForceShape.Sphere;
		p.dir_type = MPForceDirection.Radial;
		p.radial_center = pos;
		p.strength_near = strength;
		p.strength_far = 0.0f;
		p.attenuation_exp = 0.5f;
		p.range_inner = 0.0f;
		p.range_outer = radius;
		MPAPI.mpAddForce(ref p, mat);
	}
}
