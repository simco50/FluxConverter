#include "stdafx.h"
#include "TriangleMeshDesc.h"

namespace PhysxNet
{
	TriangleMeshDesc::TriangleMeshDesc(array<PxVec3>^ vertices, array<int>^ indices) :
		Vertices(vertices), Indices(indices)
	{
		
	}
	TriangleMeshDesc::TriangleMeshDesc(System::Collections::Generic::List<PxVec3>^ vertices, System::Collections::Generic::List<int>^ indices) :
		TriangleMeshDesc(vertices->ToArray(), indices->ToArray())
	{
		
	}

	physx::PxTriangleMeshDesc TriangleMeshDesc::Unmanaged()
	{
		if (Vertices->Length == 0 || Indices->Length == 0)
			throw gcnew System::Exception("No vertices or indices!");

		physx::PxTriangleMeshDesc triangleMeshDesc;
		triangleMeshDesc.setToDefault();

		triangleMeshDesc.points.count = Vertices->Length;
		pin_ptr<float> vertexPtr = &(Vertices[0].X);
		triangleMeshDesc.points.data = vertexPtr;
		triangleMeshDesc.points.stride = 3 * sizeof(float);

		triangleMeshDesc.triangles.count = Indices->Length / 3;
		pin_ptr<int> indexPtr = &(Indices[0]);
		triangleMeshDesc.triangles.data = indexPtr;
		triangleMeshDesc.triangles.stride = sizeof(int) * 3;

		if (triangleMeshDesc.isValid() == false)
			throw gcnew System::Exception("Triangle Mesh Description is invalid!");

		return triangleMeshDesc;
	}
}