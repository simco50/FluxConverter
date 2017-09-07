#include "stdafx.h"
#include "CookingParams.h"
#include "ToleranceScale.h"

namespace PhysxNet
{
	CookingParams::CookingParams():
		CookingParams::CookingParams(gcnew ToleranceScale())
	{
	}


	CookingParams::CookingParams(ToleranceScale^ toleranceScale):
		m_pToleranceScale(toleranceScale)
	{

	}

	physx::PxCookingParams PhysxNet::CookingParams::ToUnmanaged()
	{
		physx::PxCookingParams params(m_pToleranceScale->Unmanaged());
		params.convexMeshCookingType = physx::PxConvexMeshCookingType::eQUICKHULL;
		params.meshPreprocessParams = static_cast<physx::PxMeshPreprocessingFlag::Enum>(MeshPreprocessingFlags);
		params.meshWeldTolerance = MeshWeldTolerance;
		params.meshCookingHint = static_cast<physx::PxMeshCookingHint::Enum>(MeshCookingHint);
		return params;
	}

}