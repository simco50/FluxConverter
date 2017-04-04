#include "stdafx.h"
#include "Physics.h"
#include "Foundation.h"
#include "ToleranceScale.h"

namespace PhysxNet
{
	Physics::Physics(Foundation^ pFoundation):
		Physics::Physics(pFoundation, gcnew ToleranceScale())
	{
	
	}

	Physics::Physics(Foundation^ pFoundation, ToleranceScale^ pToleranceScale):
		m_pFoundation(pFoundation)
	{
		m_pPhysicsUnmanaged = PxCreatePhysics(PX_PHYSICS_VERSION, *pFoundation->GetUnmanaged(), pToleranceScale->ToUnmanaged());
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