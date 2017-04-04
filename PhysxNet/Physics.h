#pragma once

namespace PhysxNet
{
	ref class Foundation;

	public ref class Physics
	{
	public:
		Physics(Foundation^ pFoundation);
		void Release();

		physx::PxPhysics* GetUnmanaged() { return m_pPhysicsUnmanaged; }

	private:
		physx::PxPhysics* m_pPhysicsUnmanaged = nullptr;
		Foundation^ m_pFoundation;
	};
}