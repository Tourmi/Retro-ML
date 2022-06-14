using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.SuperMarioBros.Configuration;
using Retro_ML.SuperMarioBros.Game;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.SuperMarioBros
{
    [TestFixture]
    internal class DataFetcherTest
    {
        delegate bool TestFunction();

        private SMBDataFetcher? dataFetcher;
        private MockEmulatorAdapter? mockEmulatorAdapter;

        [SetUp]
        public void SetUp()
        {
            mockEmulatorAdapter = new MockEmulatorAdapter();
            dataFetcher = new SMBDataFetcher(mockEmulatorAdapter, new NeuralConfig(), new SMBPluginConfig());
            dataFetcher.NextFrame();
        }

        [Test]
        public void FrameCache()
        {
            Assert.False(dataFetcher!.CanAct());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x08);
            Assert.False(dataFetcher!.CanAct(), "The cache should not have updated");
            dataFetcher!.NextFrame();
            Assert.True(dataFetcher!.CanAct(), "The cache should have updated");
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x02);
            dataFetcher!.NextState();
            Assert.False(dataFetcher!.CanAct(), "The cache should have updated");
        }

        [Test]
        public void NextFrame()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextFrame());
        }

        [Test]
        public void NextLevel()
        {
            Assert.DoesNotThrow(() => dataFetcher!.NextState());
        }

        [Test]
        public void GetCurrentScreen()
        {
            Assert.AreEqual(0, dataFetcher!.GetMarioPositionX());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x02);
            dataFetcher.NextFrame();
            Assert.AreEqual(0x02, dataFetcher!.GetCurrentScreen());
        }

        [Test]
        public void GetPositionX()
        {
            Assert.AreEqual(0, dataFetcher!.GetMarioPositionX());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioPositionX.Address, 0x9B);
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.CurrentScreen.Address, 0x02);
            dataFetcher.NextFrame();
            Assert.AreEqual(0x9B + (0x100 * 0x02), dataFetcher!.GetMarioPositionX());
        }

        [Test]
        public void GetPositionY()
        {
            Assert.AreEqual(0, dataFetcher!.GetMarioPositionY());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioPositionY.Address, 0xC8);
            dataFetcher.NextFrame();
            Assert.AreEqual(0xC8, dataFetcher!.GetMarioPositionY());
        }

        [Test]
        public void GetScreenPositionX()
        {
            Assert.AreEqual(0, dataFetcher!.GetMarioPositionX());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioScreenPositionX.Address, 0x9B);
            dataFetcher.NextFrame();
            Assert.AreEqual(0x9B, dataFetcher!.GetMarioScreenPositionX());
        }

        [Test]
        public void GetScreenPositionY()
        {
            Assert.AreEqual(0, dataFetcher!.GetMarioPositionY());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioScreenPositionY.Address, 0xC8);
            dataFetcher.NextFrame();
            Assert.AreEqual(0xC8, dataFetcher!.GetMarioScreenPositionY());
        }

        [Test]
        public void IsSpritePresent()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x00, 0x00, 0x01, 0x00, 0x00 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.IsSpritePresent());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.IsSpritePresent.Address, new byte[] { 0x00, 0x00, 0x01, 0x00, 0x00 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.IsSpritePresent());
        }

        [Test]
        public void GetSpriteHitbox()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetSpriteHitbox());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.SpriteHitbox.Address, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetSpriteHitbox());
        }

        [Test]
        public void GetSpritePositionX()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x00, 0xAA, 0xAA, 0xAA, 0x00 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetSpritePositionX());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.SpritePositionX.Address, new byte[] { 0x00, 0xAA, 0xAA, 0xAA, 0x00 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetSpritePositionX());
        }

        [Test]
        public void GetSpritePositionY()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x00, 0xAA, 0xAA, 0xAA, 0x00 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetSpritePositionY());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.SpritePositionY.Address, new byte[] { 0x00, 0xAA, 0xAA, 0xAA, 0x00 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetSpritePositionY());
        }

        [Test]
        public void GetSpriteScreenPosition()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x00, 0xAA, 0xAA, 0xAA, 0x00 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetSpriteScreenPosition());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.SpriteScreenPosition.Address, new byte[] { 0x00, 0xAA, 0xAA, 0xAA, 0x00 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetSpriteScreenPosition());
        }

        [Test]
        public void GetFirebarAngle()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x00, 0x08, 0x08, 0x08, 0x00 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetFirebarAngle());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.FirebarSpinAngle.Address, new byte[] { 0x00, 0x08, 0x08, 0x08, 0x00 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetFirebarAngle());
        }

        [Test]
        public void GetSprites()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x02, 0x08, 0x08, 0x08, 0xAA };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetSprites());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.SpriteType.Address, new byte[] { 0x02, 0x08, 0x08, 0x08, 0xAA });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetSprites());
        }

        [Test]
        public void IsHammerPresent()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.IsHammerPresent());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.IsHammerPresent.Address, new byte[] { 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.IsHammerPresent());
        }

        [Test]
        public void GetHammerHitbox()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetHammerHitbox());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.HammerHitbox.Address, new byte[] { 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetHammerHitbox());
        }

        [Test]
        public void IsPowerUpPresent()
        {
            Assert.AreEqual(0, dataFetcher!.IsPowerUpPresent());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.IsPowerUpPresent.Address, 0x2E);
            dataFetcher.NextFrame();
            Assert.AreEqual(0x2E, dataFetcher!.IsPowerUpPresent());
        }

        [Test]
        public void GetPowerUpHitbox()
        {
            byte[] spriteArrayEmpty = { 0x00, 0x00, 0x00, 0x00 };
            byte[] spriteArrayPresent = { 0x02, 0x08, 0x08, 0x08 };
            Assert.AreEqual(spriteArrayEmpty, dataFetcher!.GetPowerUpHitbox());
            mockEmulatorAdapter!.SetMemory(Addresses.SpriteAddresses.PowerUpHitbox.Address, new byte[] { 0x02, 0x08, 0x08, 0x08 });
            dataFetcher.NextFrame();
            Assert.AreEqual(spriteArrayPresent, dataFetcher!.GetPowerUpHitbox());
        }

        [Test]
        public void IsOnGround()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioFloatState.Address, 0x00);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsOnGround());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioFloatState.Address, 0x01);
            dataFetcher.NextFrame();
            Assert.IsFalse(dataFetcher!.IsOnGround());
        }

        [Test]
        public void CanAct()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x08);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.CanAct());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x07);
            dataFetcher.NextFrame();
            Assert.IsFalse(dataFetcher!.CanAct());
        }

        [Test]
        public void IsDead()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x0B);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsDead());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x08);
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.IsFallingToDeath.Address, 0x01);
            dataFetcher.NextFrame();
            Assert.IsTrue(dataFetcher!.IsDead());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.IsFallingToDeath.Address, 0x00);
            dataFetcher.NextFrame();
            Assert.IsFalse(dataFetcher!.IsDead());
        }

        [Test]
        public void WonLevel()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x04);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.WonLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x05);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.WonLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x08);
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioFloatState.Address, 0x03);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.WonLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioFloatState.Address, 0x00);
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.WonCondition.Address, 0x02);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.WonLevel());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.WonCondition.Address, 0x00);
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.WonLevel());
        }

        [Test]
        public void IsInWater()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.IsSwimming.Address, 0x00);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsInWater());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.IsSwimming.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsInWater());
        }

        [Test]
        public void IsFlashing()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x0A);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsFlashing());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioActionState.Address, 0x08);
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsFlashing());
        }

        [Test]
        public void IsAtMaxSpeed()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.IsSwimming.Address, 0x01);
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioXSpeed.Address, 0x28);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsAtMaxSpeed());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioXSpeed.Address, 0x20);
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsAtMaxSpeed());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.IsSwimming.Address, 0x00);
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioXSpeed.Address, 0x18);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsAtMaxSpeed());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioXSpeed.Address, 0x10);
            dataFetcher!.NextFrame();
            Assert.IsFalse(dataFetcher!.IsAtMaxSpeed());
        }

        [Test]
        public void IsWaterLevel()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.LevelType.Address, 0x01);
            dataFetcher!.NextFrame();
            Assert.IsTrue(dataFetcher!.IsWaterLevel());
        }

        [Test]
        public void GetCoins()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.Coins.Address, 55);
            dataFetcher!.NextFrame();
            Assert.AreEqual(55, dataFetcher!.GetCoins());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.Coins.Address, 13);
            dataFetcher.NextFrame();
            Assert.AreEqual(13, dataFetcher!.GetCoins());
        }

        [Test]
        public void GetLives()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.Lives.Address, 5);
            dataFetcher!.NextFrame();
            Assert.AreEqual(5, dataFetcher!.GetLives());
            mockEmulatorAdapter!.SetMemory(Addresses.GameAddresses.Lives.Address, 10);
            dataFetcher.NextFrame();
            Assert.AreEqual(10, dataFetcher!.GetLives());
        }

        [Test]
        public void GetPowerUp()
        {
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioPowerupState.Address, 1);
            dataFetcher!.NextFrame();
            Assert.AreEqual(1, dataFetcher!.GetPowerUp());
            mockEmulatorAdapter!.SetMemory(Addresses.PlayerAddresses.MarioPowerupState.Address, 2);
            dataFetcher.NextFrame();
            Assert.AreEqual(2, dataFetcher!.GetPowerUp());
        }

        [Test]
        public void GetWalkableTilesAroundPosition()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void GetDangerousTilesAroundPosition()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void GetNearbyTiles()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void GetGoodTilesAroundPosition()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void DrawSpriteTiles()
        {
            Assert.Ignore("Not implemented yet");
        }

        [Test]
        public void DrawFirebarTiles()
        {
            Assert.Ignore("Not implemented yet");
        }

    }
}
