#pragma once

namespace PhysxNet
{
	ref class Foundation;
	ref class ToleranceScale;

	public ref class Physics
	{
	public:
		Physics(Foundation^ Foundation);
		Physics(Foundation^ Foundation, ToleranceScale^ ToleranceScale);
		void Release();

		physx::PxPhysics* Unmanaged() { return m_pPhysicsUnmanaged; }

	private:
		physx::PxPhysics* m_pPhysicsUnmanaged = nullptr;
		Foundation^ m_pFoundation;
	};
}