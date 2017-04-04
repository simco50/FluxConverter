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

	enum MeshCookingHint
	{
		eSIM_PERFORMANCE = 0,
		eCOOKING_PERFORMANCE = 1
	};

	ref class ToleranceScale;

	public ref class CookingParams
	{
	public:

		CookingParams();
		CookingParams(ToleranceScale^ toleranceScale);


		physx::PxCookingParams ToUnmanaged();

		MeshPreprocessingFlag MeshPreprocessingFlags;
		MeshCookingHint MeshCookingHint = eSIM_PERFORMANCE;
		float MeshWeldTolerance = 0.0f;

	private:
		ToleranceScale^ m_pToleranceScale;
	};

}
