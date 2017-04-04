#pragma once
namespace PhysxNet
{
	public ref class Foundation
	{
	public:
		Foundation();
		void Release();

		physx::PxFoundation* GetUnmanaged() { return m_pFoundationUnmanaged; }

	private:
		physx::PxFoundation* m_pFoundationUnmanaged = nullptr;

		physx::PxDefaultAllocator* m_pDefaultAllocatorUnmanaged = nullptr;
		physx::PxDefaultErrorCallback* m_pDefaultErrorCallbackUnmanaged = nullptr;
	};

}
