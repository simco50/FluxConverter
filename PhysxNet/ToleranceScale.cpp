#include "stdafx.h"
#include "ToleranceScale.h"

namespace PhysxNet
{
	ToleranceScale::ToleranceScale() : ToleranceScale(1,1000,9.81f)
	{
		
	}

	ToleranceScale::ToleranceScale(float length, float mass, float speed):
		Length(length), Mass(mass), Speed(speed)
	{
	}

	physx::PxTolerancesScale ToleranceScale::ToUnmanaged()
	{
		physx::PxTolerancesScale scale;
		scale.length = Length;
		scale.mass = Mass;
		scale.speed = Speed;
		return scale;
	}

	bool ToleranceScale::IsValid()
	{
		return ToUnmanaged().isValid();
	}
}
