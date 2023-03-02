import math

class Vector3:
    def __init__(self, x=0.0, y=0.0, z=0.0):
        self.x = x
        self.y = y
        self.z = z
        
    def __add__(self, other):
        return Vector3(self.x + other.x, self.y + other.y, self.z + other.z)

    def __sub__(self, other):
        return Vector3(self.x - other.x, self.y - other.y, self.z - other.z)

    def __mul__(self, other):
        if isinstance(other, int) or isinstance(other, float):
            return Vector3(self.x * other, self.y * other, self.z * other)
        elif isinstance(other, Vector3):
            return Vector3(self.x * other.x, self.y * other.y, self.z * other.z)

    def __truediv__(self, other):
        if isinstance(other, int) or isinstance(other, float):
            return Vector3(self.x / other, self.y / other, self.z / other)
        elif isinstance(other, Vector3):
            return Vector3(self.x / other.x, self.y / other.y, self.z / other.z)

    def __eq__(self, other):
        if isinstance(other, Vector3):
            return self.x == other.x and self.y == other.y and self.z == other.z
        return False

    def __ne__(self, other):
        return not self == other

    def __neg__(self):
        return Vector3(-self.x, -self.y, -self.z)

    def __str__(self):
        return f"Vector3({self.x}, {self.y}, {self.z})"

    def magnitude(self):
        return math.sqrt(self.x**2 + self.y**2 + self.z**2)

    def normalize(self):
        mag = self.magnitude()
        if mag == 0:
            return Vector3(0, 0, 0)
        else:
            return self / mag

    def dot(self, other):
        return self.x*other.x + self.y*other.y + self.z*other.z

    def cross(self, other):
        return Vector3(self.y*other.z - self.z*other.y, self.z*other.x - self.x*other.z, self.x*other.y - self.y*other.x)

class Vector2:
    def __init__(self, x=0.0, y=0.0):
        self.x = x
        self.y = y

    def __add__(self, other):
        return Vector2(self.x + other.x, self.y + other.y)

    def __sub__(self, other):
        return Vector3(self.x - other.x, self.y - other.y)

    def __mul__(self, other):
        if isinstance(other, int) or isinstance(other, float):
            return Vector3(self.x * other, self.y * other)
        elif isinstance(other, Vector2):
            return Vector3(self.x * other.x, self.y * other.y)

    def __truediv__(self, other):
        if isinstance(other, int) or isinstance(other, float):
            return Vector3(self.x / other, self.y / other)
        elif isinstance(other, Vector3):
            return Vector3(self.x / other.x, self.y / other.y)

    def __eq__(self, other):
        if isinstance(other, Vector3):
            return self.x == other.x and self.y == other.y
        return False

    def __ne__(self, other):
        return not self == other

    def __neg__(self):
        return Vector2(-self.x, -self.y)

    def __str__(self):
        return f"Vector2({self.x}, {self.y}, {self.z})"

class Quaternion:
    def __init__(self, x=0.0, y=0.0, z=0.0, w=0.0):
        self.x = x
        self.y = y
        self.z = z
        self.w = w

    def __mul__(self, other):
        if isinstance(other, Quaternion):
            x = self.w*other.x + self.x*other.w + self.y*other.z - self.z*other.y
            y = self.w*other.y - self.x*other.z + self.y*other.w + self.z*other.x
            z = self.w*other.z + self.x*other.y - self.y*other.x + self.z*other.w
            w = self.w*other.w - self.x*other.x - self.y*other.y - self.z*other.z
            return Quaternion(x, y, z, w)
        elif isinstance(other, (int, float)):
            return Quaternion(self.x*other, self.y*other, self.z*other, self.w*other)

    def __add__(self, other):
        if isinstance(other, Quaternion):
            return Quaternion(self.x+other.x, self.y+other.y, self.z+other.z, self.w+other.w)

    def __sub__(self, other):
        if isinstance(other, Quaternion):
            return Quaternion(self.x-other.x, self.y-other.y, self.z-other.z, self.w-other.w)

    def __neg__(self):
        return Quaternion(-self.x, -self.y, -self.z, -self.w)

    def __eq__(self, other):
        if isinstance(other, Quaternion):
            return self.x == other.x and self.y == other.y and self.z == other.z and self.w == other.w
        return False

    def __ne__(self, other):
        return not self == other

    def __str__(self):
        return f"({self.x}, {self.y}, {self.z}, {self.w})"

    def conjugate(self):
        return Quaternion(-self.x, -self.y, -self.z, self.w)

    def inverse(self):
        mag_sq = self.x**2 + self.y**2 + self.z**2 + self.w**2
        if mag_sq == 0:
            return None
        else:
            inv_mag_sq = 1 / mag_sq
            return Quaternion(-self.x*inv_mag_sq, -self.y*inv_mag_sq, -self.z*inv_mag_sq, self.w*inv_mag_sq)

    def rotate(self, vec3):
        qv = Quaternion(vec3.x, vec3.y, vec3.z, 0)
        qr = self * qv * self.conjugate()
        return Vector3(qr.x, qr.y, qr.z)