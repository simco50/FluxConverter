#include "stdafx.h"
#include "CookingParams.h"
#include "ToleranceScale.h"

namespace PhysxNet
{

	CookingParams::CookingParams(ToleranceScale^ toleranceScale):
		m_pToleranceScale(toleranceScale)
	{

	}

	physx::PxCookingParams PhysxNet::CookingParams::ToUnmanaged()
	{
		physx::PxCookingParams params(m_pToleranceScale->ToUnmanaged());
		params.convexMeshCookingType = physx::PxConvexMeshCookingType::eQUICKHULL;
		params.meshPreprocessParams = static_cast<physx::PxMeshPreprocessingFlags>(static_cast<int>(MeshPreprocessingFlags));
		params.meshWeldTolerance = MeshWeldTolerance;
		return params;
	}

}