#include "stdafx.h"
#include "Physics.h"
#include "Foundation.h"

namespace PhysxNet
{
	Physics::Physics(Foundation^ pFoundation):
		m_pFoundation(pFoundation)
	{
		physx::PxTolerancesScale scale;
		scale.mass = 1.0f;
		scale.speed = 9.81f;
		scale.length = 1.0f;
		m_pPhysicsUnmanaged = PxCreatePhysics(PX_PHYSICS_VERSION, *pFoundation->GetUnmanaged(), scale);
		if (m_pPhysicsUnmanaged == nullptr)
			throw gcnew System::Exception("Failed to create physics!");
	}

	void Physics::Release()
	{
		if (m_pPhysicsUnmanaged)
		{
			m_pPhysicsUnmanaged->release();
			m_pPhysicsUnmanaged = nullptr;
		}
	}
}