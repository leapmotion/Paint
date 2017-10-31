using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Leap.Unity.LeapPaint_v3 {


  public static class PlyExporter {

    private static MeshFilter[] _meshFilters;

    private static StringBuilder plyHeader;
    private static StringBuilder plyContentVerts;
    private static StringBuilder plyContentFaces;

    public static string MakePly(GameObject meshParentObj) {
    
      plyContentVerts = new StringBuilder();
      plyContentFaces = new StringBuilder();
      int totalNumVerts = 0;
      int totalNumIndices = 0;
      _meshFilters = meshParentObj.GetComponentsInChildren<MeshFilter>();
      for (int i = 0; i < _meshFilters.Length; i++) {
        Mesh curMesh = _meshFilters[i].mesh;

        Vector3[] curVerts = curMesh.vertices;
        Color[] curColors = curMesh.colors;
        int[] curIndices = curMesh.GetIndices(0);

        for (int j = 0; j < curVerts.Length; j++) {
          AppendVertex(plyContentVerts, curVerts[j], curColors[j]);
			  }
        for (int j = 0; j < curIndices.Length - 2; j += 3) {
          AppendFace(plyContentFaces, curIndices[j] + totalNumVerts, curIndices[j + 1] + totalNumVerts, curIndices[j + 2] + totalNumVerts);
        }
        totalNumVerts += curVerts.Length;
        totalNumIndices += curIndices.Length;
      }

      plyHeader = new StringBuilder();
      AppendHeader(plyHeader, totalNumVerts, totalNumIndices / 3);

      return plyHeader.ToString() + plyContentVerts.ToString() + plyContentFaces.ToString();
    }

    private static void AppendVertex(StringBuilder buffer, Vector3 vertex, Color color) {
      buffer.Append(vertex.x);
      buffer.Append(" ");
      buffer.Append(vertex.y);
      buffer.Append(" ");
      buffer.Append(vertex.z);
      buffer.Append(" ");
      buffer.Append((int)(Mathf.Round(color.r * 255)));
      buffer.Append(" ");
      buffer.Append((int)(Mathf.Round(color.g * 255)));
      buffer.Append(" ");
      buffer.Append((int)(Mathf.Round(color.b * 255)));
      buffer.Append("\n");
    }

    private static void AppendFace(StringBuilder buffer, int index1, int index2, int index3) {
      buffer.Append(3); // number of indices per face
      buffer.Append(" ");
      buffer.Append(index1);
      buffer.Append(" ");
      buffer.Append(index2);
      buffer.Append(" ");
      buffer.Append(index3);
      buffer.Append("\n");
    }

    private static void AppendHeader(StringBuilder buffer, int numVerts, int numFaces) {
      // Content header
      buffer.AppendLine("ply");
      buffer.AppendLine("format ascii 1.0");
      buffer.AppendLine("comment made with Paintform");

      // Vertex element definition
      buffer.AppendLine("element vertex " + numVerts);
      buffer.AppendLine("property float x");
      buffer.AppendLine("property float y");
      buffer.AppendLine("property float z");
      buffer.AppendLine("property uchar red");
      buffer.AppendLine("property uchar green");
      buffer.AppendLine("property uchar blue");

      // Face element definition
      buffer.AppendLine("element face " + numFaces);
      buffer.AppendLine("property list uchar int vertex_index");

      buffer.AppendLine("end_header");
    }

  }


}