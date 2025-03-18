using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Models;
using USDSceneOptimizer;

namespace USDOptimizer.Core.Analysis.Implementations
{
    /// <summary>
    /// Main scene analyzer that integrates all analysis components
    /// </summary>
    public class SceneAnalyzer : ISceneAnalyzer
    {
        private readonly IMeshAnalyzer _meshAnalyzer;
        private readonly IMaterialAnalyzer _materialAnalyzer;
        private readonly ISceneHierarchyAnalyzer _hierarchyAnalyzer;

        public SceneAnalyzer(
            IMeshAnalyzer meshAnalyzer, 
            IMaterialAnalyzer materialAnalyzer, 
            ISceneHierarchyAnalyzer hierarchyAnalyzer)
        {
            _meshAnalyzer = meshAnalyzer ?? throw new ArgumentNullException(nameof(meshAnalyzer));
            _materialAnalyzer = materialAnalyzer ?? throw new ArgumentNullException(nameof(materialAnalyzer));
            _hierarchyAnalyzer = hierarchyAnalyzer ?? throw new ArgumentNullException(nameof(hierarchyAnalyzer));
        }

        /// <summary>
        /// Analyzes the current scene and returns analysis results
        /// </summary>
        public AnalysisResults AnalyzeScene()
        {
            try
            {
                // Get the active scene
                var scene = new Scene
                {
                    Name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    // Add other scene initialization
                };

                // Populate scene data
                PopulateSceneData(scene);

                // Perform analysis
                var results = new AnalysisResults
                {
                    Recommendations = new List<OptimizationRecommendation>()
                };

                // Analyze meshes
                var meshTask = _meshAnalyzer.AnalyzeMeshesAsync(scene);
                meshTask.Wait();
                var meshMetrics = meshTask.Result;
                
                // Analyze materials
                var materialTask = _materialAnalyzer.AnalyzeMaterialsAsync(scene);
                materialTask.Wait();
                var materialMetrics = materialTask.Result;
                
                // Analyze hierarchy
                var hierarchyTask = _hierarchyAnalyzer.AnalyzeHierarchyAsync(scene);
                hierarchyTask.Wait();
                var hierarchyMetrics = hierarchyTask.Result;

                // Aggregate metrics
                results.TotalPolygons = meshMetrics.TotalPolygonCount;
                results.TotalVertices = meshMetrics.TotalVertexCount;
                results.TotalMaterials = materialMetrics.TotalMaterialCount;
                results.TotalTextures = materialMetrics.MaterialTextureUsage.Count;
                results.TotalMemoryUsage = CalculateTotalMemoryUsage(meshMetrics, materialMetrics);

                // Generate recommendations
                results.Recommendations.AddRange(GenerateMeshRecommendations(meshMetrics));
                results.Recommendations.AddRange(GenerateMaterialRecommendations(materialMetrics));
                results.Recommendations.AddRange(GenerateHierarchyRecommendations(hierarchyMetrics));

                return results;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing scene: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Analyzes a specific GameObject in the scene
        /// </summary>
        public GameObjectAnalysisResults AnalyzeGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            try
            {
                // Create a mini-scene with just this GameObject
                var scene = new Scene
                {
                    Name = gameObject.name,
                    // Add other scene initialization
                };

                // Add this GameObject to the scene
                PopulateSceneDataFromGameObject(scene, gameObject);

                // Perform analysis
                var results = new GameObjectAnalysisResults
                {
                    GameObjectName = gameObject.name,
                    Recommendations = new List<OptimizationRecommendation>()
                };

                // Analyze meshes
                var meshTask = _meshAnalyzer.AnalyzeMeshesAsync(scene);
                meshTask.Wait();
                var meshMetrics = meshTask.Result;
                
                // Analyze materials
                var materialTask = _materialAnalyzer.AnalyzeMaterialsAsync(scene);
                materialTask.Wait();
                var materialMetrics = materialTask.Result;

                // Aggregate metrics
                results.PolygonCount = meshMetrics.TotalPolygonCount;
                results.VertexCount = meshMetrics.TotalVertexCount;
                results.MaterialCount = materialMetrics.TotalMaterialCount;
                results.MemoryUsage = CalculateTotalMemoryUsage(meshMetrics, materialMetrics);

                // Generate recommendations
                results.Recommendations.AddRange(GenerateMeshRecommendations(meshMetrics));
                results.Recommendations.AddRange(GenerateMaterialRecommendations(materialMetrics));

                return results;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing GameObject {gameObject.name}: {ex.Message}");
                throw;
            }
        }

        #region Private Helper Methods

        private void PopulateSceneData(Scene scene)
        {
            // Populate the scene with data from the Unity scene
            // This would extract meshes, materials, and hierarchy from the current scene
            
            // Get all renderers in the scene
            Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
            
            // Extract mesh data
            scene.Meshes = new List<Mesh>();
            scene.Materials = new List<Material>();
            
            foreach (var renderer in renderers)
            {
                // Add meshes
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    scene.Meshes.Add(new Mesh 
                    { 
                        Name = meshFilter.sharedMesh.name,
                        VertexCount = meshFilter.sharedMesh.vertexCount,
                        PolygonCount = meshFilter.sharedMesh.triangles.Length / 3
                    });
                }
                
                // Add materials
                foreach (var unityMaterial in renderer.sharedMaterials)
                {
                    if (unityMaterial != null)
                    {
                        scene.Materials.Add(new Material
                        {
                            Name = unityMaterial.name
                        });
                    }
                }
            }
            
            // Build scene hierarchy
            scene.RootNode = BuildSceneHierarchy();
        }

        private void PopulateSceneDataFromGameObject(Scene scene, GameObject gameObject)
        {
            // Similar to PopulateSceneData but only for a specific GameObject and its children
            scene.Meshes = new List<Mesh>();
            scene.Materials = new List<Material>();
            
            // Get all renderers in the GameObject hierarchy
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            
            foreach (var renderer in renderers)
            {
                // Add meshes
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    scene.Meshes.Add(new Mesh 
                    { 
                        Name = meshFilter.sharedMesh.name,
                        VertexCount = meshFilter.sharedMesh.vertexCount,
                        PolygonCount = meshFilter.sharedMesh.triangles.Length / 3
                    });
                }
                
                // Add materials
                foreach (var unityMaterial in renderer.sharedMaterials)
                {
                    if (unityMaterial != null)
                    {
                        scene.Materials.Add(new Material
                        {
                            Name = unityMaterial.name
                        });
                    }
                }
            }
            
            // Build scene hierarchy
            scene.RootNode = BuildSceneHierarchyFromGameObject(gameObject);
        }

        private Node BuildSceneHierarchy()
        {
            // Get all root GameObjects in the scene
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            // Create a dummy root node
            var rootNode = new Node
            {
                Name = "SceneRoot",
                Children = new List<Node>()
            };
            
            // Add children
            foreach (var obj in rootObjects)
            {
                rootNode.Children.Add(BuildNodeFromGameObject(obj));
            }
            
            return rootNode;
        }

        private Node BuildSceneHierarchyFromGameObject(GameObject gameObject)
        {
            return BuildNodeFromGameObject(gameObject);
        }

        private Node BuildNodeFromGameObject(GameObject gameObject)
        {
            var node = new Node
            {
                Name = gameObject.name,
                Children = new List<Node>()
            };
            
            // Add children
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var childObj = gameObject.transform.GetChild(i).gameObject;
                node.Children.Add(BuildNodeFromGameObject(childObj));
            }
            
            return node;
        }

        private float CalculateTotalMemoryUsage(MeshAnalysisMetrics meshMetrics, MaterialAnalysisMetrics materialMetrics)
        {
            // Estimate memory usage based on vertex count and texture memory
            float meshMemory = meshMetrics.TotalVertexCount * 0.1f; // Approximate bytes per vertex
            float materialMemory = materialMetrics.TotalTextureMemoryBytes;
            
            return meshMemory + materialMemory;
        }

        private List<OptimizationRecommendation> GenerateMeshRecommendations(MeshAnalysisMetrics metrics)
        {
            var recommendations = new List<OptimizationRecommendation>();
            
            // Check for high-poly meshes
            if (metrics.HighPolyMeshes.Count > 0)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "High Polygon Count Meshes",
                    Description = $"Found {metrics.HighPolyMeshes.Count} meshes with high polygon counts.",
                    Priority = OptimizationPriority.High,
                    EstimatedImprovement = 0.3f
                });
            }
            
            // Check for inefficient meshes
            if (metrics.InefficientUVMappings.Count > 0)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "Inefficient UV Mappings",
                    Description = $"Found {metrics.InefficientUVMappings.Count} meshes with inefficient UV mappings.",
                    Priority = OptimizationPriority.Medium,
                    EstimatedImprovement = 0.2f
                });
            }
            
            return recommendations;
        }

        private List<OptimizationRecommendation> GenerateMaterialRecommendations(MaterialAnalysisMetrics metrics)
        {
            var recommendations = new List<OptimizationRecommendation>();
            
            // Check for high texture usage
            if (metrics.HighTextureUsageMaterials.Count > 0)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "High Texture Usage",
                    Description = $"Found {metrics.HighTextureUsageMaterials.Count} materials with excessive texture usage.",
                    Priority = OptimizationPriority.Medium,
                    EstimatedImprovement = 0.25f
                });
            }
            
            // Check for complex shaders
            if (metrics.ComplexShaderMaterials.Count > 0)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "Complex Shaders",
                    Description = $"Found {metrics.ComplexShaderMaterials.Count} materials with complex shaders.",
                    Priority = OptimizationPriority.High,
                    EstimatedImprovement = 0.35f
                });
            }
            
            return recommendations;
        }

        private List<OptimizationRecommendation> GenerateHierarchyRecommendations(SceneHierarchyMetrics metrics)
        {
            var recommendations = new List<OptimizationRecommendation>();
            
            // Check hierarchy depth
            int maxDepth = metrics.NodeDepths.Values.Max();
            if (maxDepth > 10)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "Deep Scene Hierarchy",
                    Description = $"Scene hierarchy depth of {maxDepth} exceeds recommended maximum (10).",
                    Priority = OptimizationPriority.Medium,
                    EstimatedImprovement = 0.15f
                });
            }
            
            // Check for potential instances
            if (metrics.PotentialInstanceOpportunities != null && metrics.PotentialInstanceOpportunities.Count > 0)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "Potential Instancing Opportunities",
                    Description = $"Found {metrics.PotentialInstanceOpportunities.Count} opportunities for instancing.",
                    Priority = OptimizationPriority.High,
                    EstimatedImprovement = 0.4f
                });
            }
            
            return recommendations;
        }
        
        #endregion
    }
} 