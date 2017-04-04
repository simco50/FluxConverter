#pragma once

namespace PhysxNet
{
	public value class PxVec3
	{
	public:
		PxVec3(float x, float y, float z) :
			X(x), Y(y), Z(z)
		{}
		float X, Y, Z;
	};
}