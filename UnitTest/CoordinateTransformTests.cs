using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace UnitTest
{
    [TestClass]
    public class CoordinateTransformTests : ProjNet.UnitTests.CoordinateTransformTestsBase
    {
        [TestMethod]
        public void TestTransformListOfDoubleArray()
        {
            CoordinateSystemFactory csFact = new CoordinateSystemFactory();
            CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();

            ICoordinateSystem utm35ETRS = csFact.CreateFromWkt(
                    "PROJCS[\"ETRS89 / ETRS-TM35\",GEOGCS[\"ETRS89\",DATUM[\"D_ETRS_1989\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",27],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",0],UNIT[\"Meter\",1]]");

            IProjectedCoordinateSystem utm33 = ProjectedCoordinateSystem.WGS84_UTM(33, true);

            ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(utm35ETRS, utm33);

            List<double[]> points = new List<double[]>
            {
                new[] {290586.087, 6714000 }, new[] {90586.392, 6713996.224},
                new[] {290590.133, 6713973.772}, new[] {290594.111, 6713957.416},
                new[] {290596.615, 6713943.567}, new[] {290596.701, 6713939.485}
            };

            double[][] tpoints = trans.MathTransform.TransformList(points).ToArray();
            for (int i = 0; i < points.Count; i++)
            {
                Console.WriteLine(tpoints[i]);
                NUnit.Framework.Assert.IsTrue(Equal(tpoints[i], trans.MathTransform.Transform(points[i])));
            }
        }

        private static bool Equal(IList<double> a1, IList<double> a2)
        {
            if (a2.Count != a1.Count)
                return false;

            for (int i = 0; i < a1.Count; i++)
            {
                if (!a1[i].Equals(a2[i]))
                    return false;
            }
            return true;
        }

        [TestMethod]
        public void TestCentralMeridianParse()
        {
            const string strSouthPole = "PROJCS[\"South_Pole_Lambert_Azimuthal_Equal_Area\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Lambert_Azimuthal_Equal_Area\"],PARAMETER[\"False_Easting\",0],PARAMETER[\"False_Northing\",0],PARAMETER[\"Central_Meridian\",-127],PARAMETER[\"Latitude_Of_Origin\",-90],UNIT[\"Meter\",1]]";

            CoordinateSystemFactory pCoordSysFactory = new CoordinateSystemFactory();
            ICoordinateSystem pSouthPole = pCoordSysFactory.CreateFromWkt(strSouthPole);
            NUnit.Framework.Assert.IsNotNull(pSouthPole);
        }

        [TestMethod]
        public void TestNAD27toWGS84()
        {
            CoordinateSystemFactory csFact = new CoordinateSystemFactory();
            CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();

            ICoordinateSystem ESPG32054 = csFact.CreateFromWkt(
                    "PROJCS[\"NAD27 / Wisconsin South\",GEOGCS[\"NAD27\",DATUM[\"North_American_Datum_1927\",SPHEROID[\"Clarke 1866\",6378206.4,294.9786982138982,AUTHORITY[\"EPSG\",\"7008\"]],AUTHORITY[\"EPSG\",\"6267\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4267\"]],PROJECTION[\"Lambert_Conformal_Conic_2SP\"],PARAMETER[\"standard_parallel_1\",42.73333333333333],PARAMETER[\"standard_parallel_2\",44.06666666666667],PARAMETER[\"latitude_of_origin\",42],PARAMETER[\"central_meridian\",-90],PARAMETER[\"false_easting\",2000000],PARAMETER[\"false_northing\",0],UNIT[\"US survey foot\",0.3048006096012192,AUTHORITY[\"EPSG\",\"9003\"]],AUTHORITY[\"EPSG\",\"32054\"]]");
            
            GeographicCoordinateSystem WGS84 = (ProjNet.CoordinateSystems.GeographicCoordinateSystem)GeographicCoordinateSystem.WGS84;

            ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(ESPG32054, WGS84);

            List<double[]> points = new List<double[]>
            {
                new[] { 2555658.00, 388644.00},
                new[] { 2557740.000, 387024.000}
            };
            
            for (int i = 0; i < points.Count; i++)
            {
                double[] rst = trans.MathTransform.Transform(points[i]);
                Console.WriteLine(rst[0].ToString()+" \t"+ rst[1].ToString());
            }
        }

        [TestMethod]
        public void TestDatumTransform()
        {
            //Define datums, set parameters
            HorizontalDatum wgs72 = HorizontalDatum.WGS72;
            wgs72.Wgs84Parameters = new Wgs84ConversionInfo(0, 0, 4.5, 0, 0, 0.554, 0.219);
            HorizontalDatum ed50 = HorizontalDatum.ED50;
            ed50.Wgs84Parameters = new Wgs84ConversionInfo(-81.0703, -89.3603, -115.7526,
                                                           -0.48488, -0.02436, -0.41321,
                                                           -0.540645); //Parameters for Denmark
                                                                       //Define geographic coordinate systems
            IGeographicCoordinateSystem gcsWGS72 = CoordinateSystemFactory.CreateGeographicCoordinateSystem("WGS72 Geographic", AngularUnit.Degrees, wgs72, PrimeMeridian.Greenwich,
                new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            IGeographicCoordinateSystem gcsWGS84 = CoordinateSystemFactory.CreateGeographicCoordinateSystem("WGS84 Geographic", AngularUnit.Degrees, HorizontalDatum.WGS84, PrimeMeridian.Greenwich,
                new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            IGeographicCoordinateSystem gcsED50 = CoordinateSystemFactory.CreateGeographicCoordinateSystem("ED50 Geographic", AngularUnit.Degrees, ed50, PrimeMeridian.Greenwich,
                new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            //Define geocentric coordinate systems
            IGeocentricCoordinateSystem gcenCsWGS72 = CoordinateSystemFactory.CreateGeocentricCoordinateSystem("WGS72 Geocentric", wgs72, LinearUnit.Metre, PrimeMeridian.Greenwich);
            IGeocentricCoordinateSystem gcenCsWGS84 = CoordinateSystemFactory.CreateGeocentricCoordinateSystem("WGS84 Geocentric", HorizontalDatum.WGS84, LinearUnit.Metre, PrimeMeridian.Greenwich);
            IGeocentricCoordinateSystem gcenCsED50 = CoordinateSystemFactory.CreateGeocentricCoordinateSystem("ED50 Geocentric", ed50, LinearUnit.Metre, PrimeMeridian.Greenwich);

            //Define projections
            List<ProjectionParameter> parameters = new List<ProjectionParameter>(5)
                                 {
                                     new ProjectionParameter("latitude_of_origin", 0),
                                     new ProjectionParameter("central_meridian", 9),
                                     new ProjectionParameter("scale_factor", 0.9996),
                                     new ProjectionParameter("false_easting", 500000),
                                     new ProjectionParameter("false_northing", 0)
                                 };
            IProjection projection = CoordinateSystemFactory.CreateProjection("Transverse Mercator", "Transverse_Mercator", parameters);
            IProjectedCoordinateSystem utmED50 = CoordinateSystemFactory.CreateProjectedCoordinateSystem("ED50 UTM Zone 32N", gcsED50, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));
            IProjectedCoordinateSystem utmWGS84 = CoordinateSystemFactory.CreateProjectedCoordinateSystem("WGS84 UTM Zone 32N", gcsWGS84, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            ////Set up coordinate transformations
            //var ctForw = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcsWGS72, gcenCsWGS72); //Geographic->Geocentric (WGS72)
            //var ctWGS84_Gcen2Geo = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcenCsWGS84, gcsWGS84);  //Geocentric->Geographic (WGS84)
            //var ctWGS84_Geo2UTM = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcsWGS84, utmWGS84);  //UTM ->Geographic (WGS84)
            //var ctED50_UTM2Geo = _coordinateTransformationFactory.CreateFromCoordinateSystems(utmED50, gcsED50);  //UTM ->Geographic (ED50)
            //var ctED50_Geo2Gcen = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcsED50, gcenCsED50); //Geographic->Geocentric (ED50)

            //Test datum-shift from WGS72 to WGS84
            //Point3D pGeoCenWGS72 = ctForw.MathTransform.Transform(pLongLatWGS72) as Point3D;
            double[] pGeoCenWGS72 = new[] { 3657660.66, 255768.55, 5201382.11 };

            ICoordinateTransformation geocen_ed50_2_Wgs84 = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcenCsWGS72, gcenCsWGS84);
            double[] pGeoCenWGS84 = geocen_ed50_2_Wgs84.MathTransform.Transform(pGeoCenWGS72);
            //Point3D pGeoCenWGS84 = wgs72.Wgs84Parameters.Apply(pGeoCenWGS72);
            double[] pExpected = new[] { 3657660.78, 255778.43, 5201387.75 };
            NUnit.Framework.Assert.IsTrue(ToleranceLessThan(pExpected, pGeoCenWGS84, 0.01), TransformationError("Datum WGS72->WGS84", pExpected, pGeoCenWGS84));

            //and inverse
            double[] pGeoCenWGS72calc = geocen_ed50_2_Wgs84.MathTransform.Inverse().Transform(pGeoCenWGS84);
            NUnit.Framework.Assert.IsTrue(ToleranceLessThan(pGeoCenWGS72, pGeoCenWGS72calc, 0.001), TransformationError("Datum WGS84->WGS72", pGeoCenWGS72, pGeoCenWGS72calc));

            ICoordinateTransformation utm_ed50_2_Wgs84 = CoordinateTransformationFactory.CreateFromCoordinateSystems(utmED50, utmWGS84);
            double[] pUTMED50 = new double[] { 600000, 6100000 };
            double[] pUTMWGS84 = utm_ed50_2_Wgs84.MathTransform.Transform(pUTMED50);
            pExpected = new[] { 599928.6, 6099790.2 };
            NUnit.Framework.Assert.IsTrue(ToleranceLessThan(pExpected, pUTMWGS84, 0.1), TransformationError("Datum ED50->WGS84", pExpected, pUTMWGS84));

            //and inverse
            double[] pUTMED50calc = utm_ed50_2_Wgs84.MathTransform.Inverse().Transform(pUTMWGS84);
            NUnit.Framework.Assert.IsTrue(ToleranceLessThan(pUTMED50, pUTMED50calc, 0.01), TransformationError("Datum WGS84->ED50", pUTMED50, pUTMED50calc));


            //Perform reverse
            ICoordinateTransformation utm_Wgs84_2_Ed50 = CoordinateTransformationFactory.CreateFromCoordinateSystems(utmWGS84, utmED50);
            pUTMED50 = utm_Wgs84_2_Ed50.MathTransform.Transform(pUTMWGS84);
            pExpected = new double[] { 600000, 6100000 };
            NUnit.Framework.Assert.IsTrue(ToleranceLessThan(pExpected, pUTMED50, 0.1), TransformationError("Datum", pExpected, pUTMED50));

            //and inverse
            double[] pUTMWGS84calc = utm_Wgs84_2_Ed50.MathTransform.Inverse().Transform(pUTMED50);
            NUnit.Framework.Assert.IsTrue(ToleranceLessThan(pUTMWGS84, pUTMWGS84calc, 0.1), TransformationError("Datum", pUTMWGS84, pUTMWGS84calc));


            //Assert.IsTrue(Math.Abs((pUTMWGS84 as Point3D).Z - 36.35) < 0.5);
            //Point pExpected = Point.FromDMS(2, 7, 46.38, 53, 48, 33.82);
            //ED50_to_WGS84_Denmark: datum.Wgs84Parameters = new Wgs84ConversionInfo(-89.5, -93.8, 127.6, 0, 0, 4.5, 1.2);

        }
    }
}
