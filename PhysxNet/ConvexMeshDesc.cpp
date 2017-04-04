#include "stdafx.h"
#include "ConvexMeshDesc.h"
#include "Math.h"

namespace PhysxNet
{
	ConvexMeshDesc::ConvexMeshDesc(array<PxVec3>^ vertices, array<int>^ indices) :
		Vertices(vertices), Indices(indices)
	{

	}
	ConvexMeshDesc::ConvexMeshDesc(System::Collections::Generic::List<PxVec3>^ vertices, System::Collections::Generic::List<int>^ indices) :
		ConvexMeshDesc(vertices->ToArray(), indices->ToArray())
	{

	}

	physx::PxConvexMeshDesc ConvexMeshDesc::ToUnmanaged()
	{
		if (Vertices->Length == 0 || Indices->Length == 0)
			return physx::PxConvexMeshDesc();

		physx::PxConvexMeshDesc desc;
		desc.setToDefault();
		desc.flags |= physx::PxConvexFlag::eCOMPUTE_CONVEX;
		desc.points.count = Vertices->Length;

		pin_ptr<float> vertexPtr = &(Vertices[0].X);
		desc.points.data = vertexPtr;
		desc.points.stride = 3 * sizeof(float);

		desc.indices.count = Indices->Length;
		pin_ptr<int> indexPtr = &(Indices[0]);
		desc.indices.data = indexPtr;
		desc.indices.stride = sizeof(int);

		if (desc.isValid() == false)
			throw gcnew System::Exception("Convex Mesh Description is invalid!");

		return desc;
	}
}
