#include <Arduino.h>

#define A 32
#define B 23
#define X 33
#define Y 31
#define l 24 //Left
#define r 27 //Right
#define u 26 //Up
#define d 25 //Down
#define L 28 //Left Shoulder
#define R 22 //Right Shoulder
#define S 29 //Start
#define s 30 //Select

byte* bytes = new byte[2];

void setup() {
  Serial.begin(9600);
  for(int i = 22; i <= 33; i++) {
    pinMode(i, OUTPUT);
  }
}

void loop() {
  Serial.readBytes(bytes, 2);
  int curr = 0;
  digitalWrite(A, bytes[0] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(B, bytes[0] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(X, bytes[0] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(Y, bytes[0] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(l, bytes[0] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(r, bytes[0] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(u, bytes[0] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(d, bytes[0] & 1 << curr++ ? HIGH : LOW);

  curr = 0;
  digitalWrite(L, bytes[1] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(R, bytes[1] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(S, bytes[1] & 1 << curr++ ? HIGH : LOW);
  digitalWrite(s, bytes[1] & 1 << curr++ ? HIGH : LOW);

  /*
  for(int i = 0; i <= 33; i++) {
    digitalWrite(i + 22, HIGH);
    delay(500);
    digitalWrite(i + 22, LOW);
    delay(50);
  }
  */
}