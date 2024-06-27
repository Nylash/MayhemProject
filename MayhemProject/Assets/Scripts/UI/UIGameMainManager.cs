using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameMainManager : MonoBehaviour
{
    [SerializeField] private Image _primaryWeaponIcon;
    [SerializeField] private Image _secondaryWeaponIcon;
    [SerializeField] private TextMeshProUGUI _primaryAmmunitionsText;
    [SerializeField] private TextMeshProUGUI _secondaryAmmunitionsText;
    [SerializeField] private Image _primaryReloadingImage;
    [SerializeField] private Image _secondaryReloadingImage;
    [SerializeField] private Image _healthBarImage;

    private Data_Character _characterData;

    private void Awake()
    {
        _characterData = PlayerShotManager.Instance.CharacterData;

        _characterData.event_currentHPUpdated.AddListener(UpdateHealthBar);
        _characterData.PrimaryWeapon.event_weapondIsReloadingUpdated.AddListener(PrimaryWeaponReload);
        _characterData.SecondaryWeapon.event_weapondIsReloadingUpdated.AddListener(SecondaryWeaponReload);
        _characterData.PrimaryWeapon.event_currentAmmunitionUpdated.AddListener(UpdatePrimaryWeaponAmmunitions);
        _characterData.SecondaryWeapon.event_currentAmmunitionUpdated.AddListener(UpdateSecondaryWeaponAmmunitions);

        _primaryWeaponIcon.sprite = _characterData.PrimaryWeapon.WeaponImage;
        _primaryWeaponIcon.rectTransform.localScale = Vector3.one * _characterData.PrimaryWeapon.WeaponImageSize;
        _secondaryWeaponIcon.sprite = _characterData.SecondaryWeapon.WeaponImage;
        _secondaryWeaponIcon.rectTransform.localScale = Vector3.one * _characterData.SecondaryWeapon.WeaponImageSize;
    }

    private void UpdateHealthBar(float newHealth)
    {
        _healthBarImage.fillAmount = newHealth/_characterData.MaxHP;
    }

    private void PrimaryWeaponReload(bool isReloading)
    {
        if (isReloading)
        {
            StartCoroutine(Reload(_primaryReloadingImage, _characterData.PrimaryWeapon));
        }
        else
        {
            _primaryReloadingImage.fillAmount = 0;
        }
    }

    private void SecondaryWeaponReload(bool isReloading)
    {
        if (isReloading)
        {
            StartCoroutine(Reload(_secondaryReloadingImage, _characterData.SecondaryWeapon));
        }
        else
        {
            _secondaryReloadingImage.fillAmount = 0;
        }
    }

    private void UpdatePrimaryWeaponAmmunitions(int newAmmunitions)
    {
        _primaryAmmunitionsText.text = newAmmunitions.ToString();
    }

    private void UpdateSecondaryWeaponAmmunitions(int newAmmunitions)
    {
        _secondaryAmmunitionsText.text = newAmmunitions.ToString();
    }

    private IEnumerator Reload(Image reloadImage, Data_Weapon weapon)
    {
        float timer = 0f;
        while(timer < weapon.ReloadDuration)
        {
            timer += Time.deltaTime;
            reloadImage.fillAmount =  timer / weapon.ReloadDuration;
            yield return new WaitForEndOfFrame();
        }
        yield break;
    }
}
