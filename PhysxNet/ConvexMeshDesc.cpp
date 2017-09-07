#include "stdafx.h"
#include "ConvexMeshDesc.h"

namespace PhysxNet
{
	ConvexMeshDesc::ConvexMeshDesc(array<PxVec3>^ vertices) :
		Vertices(vertices)
	{

	}
	ConvexMeshDesc::ConvexMeshDesc(System::Collections::Generic::List<PxVec3>^ vertices) :
		ConvexMeshDesc(vertices->ToArray())
	{

	}

	physx::PxConvexMeshDesc ConvexMeshDesc::Unmanaged()
	{
		if (Vertices->Length == 0)
			return physx::PxConvexMeshDesc();

		physx::PxConvexMeshDesc desc;
		desc.setToDefault();
		desc.flags |= physx::PxConvexFlag::eCOMPUTE_CONVEX;

		desc.points.count = Vertices->Length;
		pin_ptr<float> vertexPtr = &(Vertices[0].X);
		desc.points.data = vertexPtr;
		desc.points.stride = 3 * sizeof(float);

		if (desc.isValid() == false)
			throw gcnew System::Exception("Convex Mesh Description is invalid!");

		return desc;
	}
}
