#include "stdafx.h"
#include "Cooking.h"
#include "Foundation.h"
#include "TriangleMeshDesc.h"
#include "Math.h"
#include "ConvexMeshDesc.h"
#include "ToleranceScale.h"
#include "Physics.h"
#include "PhysicsMesh.h"
#include "PhysicsStreams.h"

namespace PhysxNet
{
	Cooking::Cooking(Foundation^ pFoundation, Physics^ pPhysics) :
		Cooking::Cooking(pFoundation, pPhysics, gcnew ToleranceScale())
	{
		
	}

	Cooking::Cooking(Foundation ^ pFoundation, Physics^ pPhysics, ToleranceScale ^ ToleranceScale) :
		m_pFoundation(pFoundation), m_pPhysics(pPhysics)
	{
		physx::PxCookingParams params(ToleranceScale->ToUnmanaged());

		params.meshPreprocessParams |= physx::PxMeshPreprocessingFlag::eFORCE_32BIT_INDICES;
		params.meshPreprocessParams |= physx::PxMeshPreprocessingFlag::eWELD_VERTICES;
		params.convexMeshCookingType = physx::PxConvexMeshCookingType::eQUICKHULL;
		params.meshWeldTolerance = 0.05f;
		m_pCookingUnmanaged = PxCreateCooking(PX_PHYSICS_VERSION, *pFoundation->GetUnmanaged(), params);
		if (m_pCookingUnmanaged == nullptr)
			throw gcnew System::Exception("Failed to create cooking!");
	}

	void Cooking::Release()
	{
		if (m_pCookingUnmanaged)
		{
			m_pCookingUnmanaged->release();
			m_pCookingUnmanaged = nullptr;
		}
	}

	PhysicsMesh^ Cooking::CreateConvexMesh(ConvexMeshDesc^ desc)
	{
		physx::PxConvexMeshDesc convexDesc = desc->ToUnmanaged();

		PhysicsOutputStream outputStream;
		m_pCookingUnmanaged->cookConvexMesh(convexDesc, outputStream);
		if (outputStream.GetBuffer().size() == 0)
			return nullptr;

		PhysicsMesh^ pOutputMesh = gcnew PhysicsMesh();
		std::vector<char> dataBuffer = outputStream.GetBuffer();
		pOutputMesh->MeshData->Capacity = dataBuffer.size();
		for (size_t i = 0; i < dataBuffer.size(); i++)
			pOutputMesh->MeshData->Add(dataBuffer[i]);

		PhysicsInputStream inputStream(dataBuffer);
		physx::PxConvexMesh* pConvexMesh = m_pPhysics->GetUnmanaged()->createConvexMesh(inputStream);
		if (pConvexMesh == nullptr)
			return nullptr;

		physx::PxU32 nbVerts = pConvexMesh->getNbVertices();
		const physx::PxVec3* convexVerts = pConvexMesh->getVertices();
		const physx::PxU8* indexBuffer = pConvexMesh->getIndexBuffer();
		physx::PxU32 offset = 0;
		for (physx::PxU32 i = 0; i<pConvexMesh->getNbPolygons(); i++)
		{
			physx::PxHullPolygon face;
			bool status = pConvexMesh->getPolygonData(i, face);
			PX_ASSERT(status);

			const physx::PxU8* faceIndices = indexBuffer + face.mIndexBase;
			for (physx::PxU32 j = 0; j<face.mNbVerts; j++)
			{
				pOutputMesh->Vertices->Add(PxVec3(convexVerts[faceIndices[j]].x, convexVerts[faceIndices[j]].y, convexVerts[faceIndices[j]].z));
			}

			for (physx::PxU32 j = 2; j<face.mNbVerts; j++)
			{
				pOutputMesh->Indices->Add(offset);
				pOutputMesh->Indices->Add(offset + j);
				pOutputMesh->Indices->Add(offset + j - 1);
			}
			offset += face.mNbVerts;
		}

		if(pConvexMesh->isReleasable())
			pConvexMesh->release();

		return pOutputMesh;
	}

	PhysicsMesh^ Cooking::CreateTriangleMesh(TriangleMeshDesc^ desc)
	{
		physx::PxTriangleMeshDesc triangleDesc = desc->ToUnmanaged();

		PhysicsOutputStream outputStream;
		m_pCookingUnmanaged->cookTriangleMesh(triangleDesc, outputStream);
		if (outputStream.GetBuffer().size() == 0)
			return nullptr;

		PhysicsMesh^ pOutputMesh = gcnew PhysicsMesh();
		std::vector<char> dataBuffer = outputStream.GetBuffer();
		pOutputMesh->MeshData->Capacity = dataBuffer.size();
		for (size_t i = 0; i < dataBuffer.size(); i++)
			pOutputMesh->MeshData->Add(dataBuffer[i]);

		PhysicsInputStream inputStream(dataBuffer);
		physx::PxTriangleMesh* pTriangleMesh = m_pPhysics->GetUnmanaged()->createTriangleMesh(inputStream);
		if (pTriangleMesh == nullptr)
			return nullptr;

		const physx::PxVec3* vertices = pTriangleMesh->getVertices();
		pOutputMesh->Vertices->Capacity = pTriangleMesh->getNbVertices();
		for (size_t i = 0; i < pTriangleMesh->getNbVertices(); i++)
			pOutputMesh->Vertices->Add(PxVec3(vertices[i].x, vertices[i].y, vertices[i].z));

		const int* indices = reinterpret_cast<const int*>(pTriangleMesh->getTriangles());
		pOutputMesh->Indices->Capacity = pTriangleMesh->getNbTriangles() * 3;
		for (size_t i = 0; i < pTriangleMesh->getNbTriangles() * 3; i++)
			pOutputMesh->Indices->Add(indices[i]);

		if (pTriangleMesh->isReleasable())
			pTriangleMesh->release();

		return pOutputMesh;
	}
}