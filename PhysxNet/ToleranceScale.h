#pragma once
namespace PhysxNet
{
	public ref class ToleranceScale
	{
	public:
		ToleranceScale();
		ToleranceScale(float length, float mass, float speed);

		physx::PxTolerancesScale ToUnmanaged();

		bool IsValid();

		float Length;
		float Mass;
		float Speed;
	};
}

