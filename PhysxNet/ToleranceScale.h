#pragma once
namespace PhysxNet
{
	public ref class ToleranceScale
	{
	public:
		ToleranceScale();
		ToleranceScale(float length, float mass, float speed);

		physx::PxTolerancesScale Unmanaged();

		bool IsValid();

		float Length;
		float Mass;
		float Speed;
	};
}

