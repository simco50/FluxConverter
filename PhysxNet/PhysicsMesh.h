#pragma once
namespace PhysxNet
{
	value class PxVec3;

	public ref class PhysicsMesh
	{
	public:
		System::Collections::Generic::List<unsigned char>^ MeshData = gcnew System::Collections::Generic::List<unsigned char>;
		System::Collections::Generic::List<PxVec3>^ Vertices = gcnew System::Collections::Generic::List<PxVec3>;
		System::Collections::Generic::List<int>^ Indices = gcnew System::Collections::Generic::List<int>;
	};
}

