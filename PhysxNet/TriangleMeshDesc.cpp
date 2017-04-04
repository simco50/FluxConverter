#include "stdafx.h"
#include "TriangleMeshDesc.h"
#include "Math.h"

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

	physx::PxTriangleMeshDesc TriangleMeshDesc::ToUnmanaged()
	{
		if (Vertices->Length == 0 || Indices->Length == 0)
			return physx::PxTriangleMeshDesc();

		physx::PxTriangleMeshDesc desc;
		desc.setToDefault();
		desc.points.count = Vertices->Length;

		pin_ptr<float> vertexPtr = &(Vertices[0].X);
		desc.points.data = vertexPtr;
		desc.points.stride = 3 * sizeof(float);

		desc.triangles.count = Indices->Length / 3;
		pin_ptr<int> indexPtr = &(Indices[0]);
		desc.triangles.data = indexPtr;
		desc.triangles.stride = sizeof(int) * 3;

		if (desc.isValid() == false)
			throw gcnew System::Exception("Triangle Mesh Description is invalid!");

		return desc;
	}
}