using UnityEngine;

public class KalmanFilter
{
    private float qAngle;  // Process noise variance for the accelerometer
    private float qBias;   // Process noise variance for the gyro bias
    private float rMeasure; // Measurement noise variance - this is actually the variance of the measurement noise

    private float angle;   // The angle calculated by the Kalman filter - part of the 2x1 state vector
    private float bias;    // The gyro bias calculated by the Kalman filter - part of the 2x1 state vector
    private float rate;    // Unbiased rate calculated from the rate and the calculated bias - you have to call getAngle to update the rate

    private float P00, P01, P10, P11; // Error covariance matrix - This is a 2x2 matrix

    public KalmanFilter()
    {
        qAngle = 0.001f;
        qBias = 0.003f;
        rMeasure = 0.03f;

        angle = 0.0f; // Reset the angle
        bias = 0.0f;  // Reset bias

        // Since we assume that the bias is 0 and we know the starting angle (use setAngle), the error covariance matrix is set to 0
        P00 = 0.0f;
        P01 = 0.0f;
        P10 = 0.0f;
        P11 = 0.0f;
    }

    public float GetAngle(float newAngle, float newRate, float dt)
    {
        // Discrete Kalman filter time update equations - Time Update ("Predict")
        // Update xhat - Project the state ahead
        rate = newRate - bias;
        angle += dt * rate;

        // Update estimation error covariance - Project the error covariance ahead
        P00 += dt * (dt * P11 - P01 - P10 + qAngle);
        P01 -= dt * P11;
        P10 -= dt * P11;
        P11 += qBias * dt;

        // Discrete Kalman filter measurement update equations - Measurement Update ("Correct")
        // Calculate Kalman gain - Compute the Kalman gain
        float S = P00 + rMeasure; // Estimate error
        float K0 = P00 / S;
        float K1 = P10 / S;

        // Update estimate with measurement zk (newAngle)
        float y = newAngle - angle; // Innovation or measurement residual
        angle += K0 * y;
        bias += K1 * y;

        // Update the error covariance - Update the error covariance
        float P00_temp = P00;
        float P01_temp = P01;

        P00 -= K0 * P00_temp;
        P01 -= K0 * P01_temp;
        P10 -= K1 * P00_temp;
        P11 -= K1 * P01_temp;

        return angle;
    }

    public void SetAngle(float angle) { this.angle = angle; } // Used to set angle, this should be set as the starting angle
    public float GetRate() { return rate; } // Return the unbiased rate

    public void SetQangle(float qAngle) { this.qAngle = qAngle; }
    public void SetQbias(float qBias) { this.qBias = qBias; }
    public void SetRmeasure(float rMeasure) { this.rMeasure = rMeasure; }

    public float GetQangle() { return qAngle; }
    public float GetQbias() { return qBias; }
    public float GetRmeasure() { return rMeasure; }
}