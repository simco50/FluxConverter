#pragma once

namespace PhysxNet
{
	ref class Foundation;
	ref class Physics;
	ref class TriangleMeshDesc;
	ref class ConvexMeshDesc;
	ref class ToleranceScale;
	ref class PhysicsMesh;
	ref class CookingParams;

	public ref class Cooking
	{
	public:
		Cooking(Foundation^ pFoundation, Physics^ pPhysics);
		Cooking(Foundation ^ pFoundation, Physics^ pPhysics, ToleranceScale ^ ToleranceScale);
		void Release();

		PhysicsMesh^ CreateConvexMesh(ConvexMeshDesc^ desc);
		PhysicsMesh^ CreateTriangleMesh(TriangleMeshDesc^ desc);

	private:

		physx::PxCooking* m_pCookingUnmanaged = nullptr;
		Foundation^ m_pFoundation;
		Physics^ m_pPhysics;
	};

}