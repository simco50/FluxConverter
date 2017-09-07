#pragma once
namespace PhysxNet
{
	value class PxVec3;

	public ref class TriangleMeshDesc
	{
	public:
		TriangleMeshDesc(array<PxVec3>^ vertices, array<int>^ indices);
		TriangleMeshDesc(System::Collections::Generic::List<PxVec3>^ vertices, System::Collections::Generic::List<int>^ indices);

		physx::PxTriangleMeshDesc Unmanaged();

	private:
		array<PxVec3>^ Vertices;
		array<int>^ Indices;
	};

	class ErrorReporter : public physx::PxErrorCallback
	{
		

	};
}