#include "stdafx.h"
#include "Foundation.h"

namespace PhysxNet
{
	Foundation::Foundation()
	{
		m_pDefaultAllocatorUnmanaged = new physx::PxDefaultAllocator();
		m_pDefaultErrorCallbackUnmanaged = new physx::PxDefaultErrorCallback();

		m_pFoundationUnmanaged = PxCreateFoundation(PX_FOUNDATION_VERSION, *m_pDefaultAllocatorUnmanaged, *m_pDefaultErrorCallbackUnmanaged);
		if (m_pFoundationUnmanaged == nullptr)
			throw gcnew System::Exception("Failed to create foundation!");
	}

	void Foundation::Release()
	{
		if (m_pFoundationUnmanaged)
		{
			m_pFoundationUnmanaged->release();
			m_pFoundationUnmanaged = nullptr;
		}

		delete m_pDefaultAllocatorUnmanaged;
		delete m_pDefaultErrorCallbackUnmanaged;
	}
}