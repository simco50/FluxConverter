#pragma once


namespace PhysxNet
{
	public ref class ConvexMeshDesc
	{
	public:
		ConvexMeshDesc(array<PxVec3>^ vertices, array<int>^ indices);
		ConvexMeshDesc(System::Collections::Generic::List<PxVec3>^ vertices, System::Collections::Generic::List<int>^ indices);

		physx::PxConvexMeshDesc ToUnmanaged();

	private:
		array<PxVec3>^ Vertices;
		array<int>^ Indices;
	};
}

