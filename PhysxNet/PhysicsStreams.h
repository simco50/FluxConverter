#pragma once

#include <foundation/PxIO.h>

class PhysicsOutputStream : public physx::PxOutputStream
{
public:
	PhysicsOutputStream()
	{}

	~PhysicsOutputStream()
	{
		if(m_pBuffer != nullptr)
		{
			delete[] m_pBuffer;
			m_pBuffer = nullptr;
		}
	}

	PhysicsOutputStream(const PhysicsOutputStream& other) = delete;
	PhysicsOutputStream operator=(const PhysicsOutputStream& other) = delete;

	uint32_t write(const void* src, uint32_t count) override
	{
		if (m_pBuffer == nullptr)
		{
			m_pBuffer = new char[count];
			m_Position = 0;
			m_BufferSize = count;
		}
		else
		{
			char* pNewBuffer = new char[m_BufferSize + count];
			memcpy(pNewBuffer, m_pBuffer, m_BufferSize);
			m_BufferSize += count;
			delete[] m_pBuffer;
			m_pBuffer = pNewBuffer;
		}

		const char* pData = reinterpret_cast<const char*>(src);
		for (size_t i = 0; i < count; i++)
		{
			m_pBuffer[m_Position] = pData[i];
			++m_Position;
		}
		return count;
	}

	const char* const Data() const { return m_pBuffer; }
	size_t GetCount() const { return m_Position; }
	size_t GetSize() const { return m_BufferSize; }

private:

	char* m_pBuffer = nullptr;
	size_t m_Position = 0;
	size_t m_BufferSize = 0;
};

class PhysicsInputStream : public physx::PxInputStream
{
public:
	PhysicsInputStream(const char* pData, const size_t size) :
		m_pBuffer(pData), m_Size(size), m_pCurrentPos(pData)
	{
	}

	uint32_t read(void* dest, uint32_t count) override
	{
		memcpy(dest, m_pCurrentPos, count);
		m_pCurrentPos += count;
		return count;
	}

	const char* Data() const { return m_pBuffer; }
private:

	const char* m_pBuffer;
	const char* m_pCurrentPos = nullptr;
	size_t m_Size = 0;
};