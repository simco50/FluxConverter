#pragma once

#include <foundation/PxIO.h>
#include <vector>

class PhysicsOutputStream : public physx::PxOutputStream
{
public:
	uint32_t write(const void* src, uint32_t count) override
	{
		const char* pData = reinterpret_cast<const char*>(src);
		for (size_t i = 0; i < count; i++)
			m_Buffer.push_back(pData[i]);
		return count;
	}

	std::vector<char> GetBuffer() const { return m_Buffer; }
private:
	std::vector<char> m_Buffer;
};

class PhysicsInputStream : public physx::PxInputStream
{
public:
	PhysicsInputStream(std::vector<char> data) :
		m_Buffer(data)
	{
		pCurrentPos = m_Buffer.data();
	}

	uint32_t read(void* dest, uint32_t count) override
	{
		memcpy(dest, pCurrentPos, count);
		pCurrentPos += count;
		return count;
	}

	std::vector<char> GetBuffer() const { return m_Buffer; }
private:
	std::vector<char> m_Buffer;
	char* pCurrentPos = 0;
};