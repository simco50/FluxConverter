#pragma once

namespace PhysxNet
{
	public ref class ConvexMeshDesc
	{
	public:
		ConvexMeshDesc(array<PxVec3>^ vertices);
		ConvexMeshDesc(System::Collections::Generic::List<PxVec3>^ vertices);

		physx::PxConvexMeshDesc Unmanaged();

	private:
		array<PxVec3>^ Vertices;
	};
}

