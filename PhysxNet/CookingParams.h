#pragma once
namespace PhysxNet
{
	enum MeshPreprocessingFlag
	{
		eWELD_VERTICES = 1 << 0,
		eDISABLE_CLEAN_MESH = 1 << 1,
		eDISABLE_ACTIVE_EDGES_PRECOMPUTE = 1 << 2,
		eFORCE_32BIT_INDICES = 1 << 3
	};

	ref class ToleranceScale;

	ref class CookingParams
	{
	public:
		CookingParams(ToleranceScale^ toleranceScale);

		physx::PxCookingParams ToUnmanaged();

		MeshPreprocessingFlag MeshPreprocessingFlags;
		float MeshWeldTolerance = 0.0f;

	private:
		ToleranceScale^ m_pToleranceScale;
	};

}
