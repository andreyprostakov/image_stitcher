﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Panoramas.Matching;

namespace Panoramas {
  public class Transformation {
    Bitmap image;

    public Transformation(Bitmap image) {
      this.image = image;
    }

    public Transformation(Bitmap image, KeyPointsPair[] matches) :
      this(image)
    {
      this.homography_matrix = DefineHomography(matches);
    }

    public Transformation(Transformation transformation) {
      this.homography_matrix = transformation.homography_matrix.Clone();
      this.image = transformation.image;
    }

    Emgu.CV.HomographyMatrix _homography_matrix;
    Emgu.CV.HomographyMatrix homography_matrix {
      get {
        if (_homography_matrix == null)
          _homography_matrix = NewHomography();
        return _homography_matrix;
      }
      set {
        this._homography_matrix = value;
      }
    }
    public double[,] Matrix() {
      return (double[,])homography_matrix.Data.Clone();
    }

    public Transformation Invert() {
      return (Transformation)this.MemberwiseClone();
    }

    public void Distort(Transformation outer_transformation) {
      homography_matrix[0, 2] += outer_transformation.homography_matrix[0, 2];
      homography_matrix[1, 2] += outer_transformation.homography_matrix[1, 2];
    }

    public Bitmap Transform() {
      var formatted_result = new Emgu.CV.Image<Bgr, int>(image);
      return TransformOn(formatted_result);
    }

    public Bitmap TransformOn(Emgu.CV.Image<Bgr, int> template) {
      var formatted_segment = new Emgu.CV.Image<Bgr, int>(image);
      Emgu.CV.CvInvoke.cvWarpPerspective(formatted_segment.Ptr, template.Ptr, homography_matrix.Ptr, (int)Emgu.CV.CvEnum.INTER.CV_INTER_NN, new MCvScalar(0, 0, 0));
      return template.ToBitmap();
    }

    public Bitmap TransformWithin(Emgu.CV.Image<Bgr, int> template, Transformation accum_transform) {
      var self_clone = new Transformation(this);
      self_clone.Distort(accum_transform);
      var formatted_segment = new Emgu.CV.Image<Bgr, int>(image);
      Emgu.CV.CvInvoke.cvWarpPerspective(formatted_segment.Ptr, template.Ptr, self_clone.homography_matrix.Ptr, (int)Emgu.CV.CvEnum.INTER.CV_INTER_NN, new MCvScalar(0, 0, 0));
      return template.ToBitmap();
    }

    public void Move(int x_diff, int y_diff) {
      homography_matrix[0, 2] += x_diff;
      homography_matrix[1, 2] += y_diff;
    }

    public override bool Equals(object obj) {
      if (!(obj is Transformation))
        return false;
      for (int x = 0; x < Matrix().GetLength(0); x++)
        for (int y = 0; y < Matrix().GetLength(1); y++)
          if (((Transformation)obj).Matrix()[x, y] != Matrix()[x, y])
            return false;
      return true;
    }

    Emgu.CV.HomographyMatrix NewHomography() {
      var matrix = new Emgu.CV.HomographyMatrix();
      matrix[0, 0] = 1;
      matrix[1, 1] = 1;
      matrix[2, 2] = 1;
      matrix[2, 0] = 0.0;
      matrix[0, 1] = 0.0;
      return matrix;
    }

    Emgu.CV.HomographyMatrix DefineHomography(KeyPointsPair[] matches) {
      var points_dst = matches.Select((m) => m.FeatureLeft.KeyPoint.Point).ToArray();
      var points_src = matches.Select((m) => m.FeatureRight.KeyPoint.Point).ToArray();
      var matrix = Emgu.CV.CameraCalibration.FindHomography(
        points_src,
        points_dst,
        Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.RANSAC,
        2 // RANSAC reprojection error
        );
      return matrix;
    }
  }
}
