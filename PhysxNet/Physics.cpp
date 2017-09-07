#include "stdafx.h"
#include "Physics.h"
#include "Foundation.h"
#include "ToleranceScale.h"

namespace PhysxNet
{
	Physics::Physics(Foundation^ Foundation):
		Physics::Physics(Foundation, gcnew ToleranceScale())
	{
	
	}

	Physics::Physics(Foundation^ Foundation, ToleranceScale^ ToleranceScale):
		m_pFoundation(Foundation)
	{
		m_pPhysicsUnmanaged = PxCreatePhysics(PX_PHYSICS_VERSION, *Foundation->Unmanaged(), ToleranceScale->Unmanaged());

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