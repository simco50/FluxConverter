#pragma once

namespace PhysxNet
{
	ref class Foundation;
	ref class ToleranceScale;

	public ref class Physics
	{
	public:
		Physics(Foundation^ pFoundation);
		Physics(Foundation^ pFoundation, ToleranceScale^ pToleranceScale);
		void Release();

		physx::PxPhysics* GetUnmanaged() { return m_pPhysicsUnmanaged; }

	private:
		physx::PxPhysics* m_pPhysicsUnmanaged = nullptr;
		Foundation^ m_pFoundation;
	};
}