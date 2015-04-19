﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public CharacterController character;
	public float moveSpeed;
	public float gravity;
	public float jumpForce;
	
	public Transform headTrans, bodyTrans;
	public GameObject spotlight;
	private bool flashlightOn = false;
	private float flashlightToggled = 0;
	private Vector2 rotation;

	public const int NO_AMMO = 0, YELLOW = 1, GREEN = 2, BLUE = 3;
	public float cooldown;
	public float shootForce;
	public GameObject yellowAmmo, greenAmmo, blueAmmo;
	public GUIText equipText;
	private int currentAmmo = NO_AMMO;
	private bool canShootYellow = false, canShootGreen = false, canShootBlue = false;
	private float firingCooldown;
	private string originalEquipText;

	void Start () {
		rotation.x = bodyTrans.localEulerAngles.y;
		rotation.y = bodyTrans.localEulerAngles.x;
		originalEquipText = equipText.text;
	}
	
	void OnTriggerEnter (Collider other) {
		if (other.tag.Contains ("Pickup")) {
			bool noAmmo = !canShootYellow && !canShootGreen && !canShootBlue;
			if (other.tag.Contains ("Yellow")) {
				canShootYellow = true;
				if (noAmmo) {
					currentAmmo = YELLOW;
				}
			}
			if (other.tag.Contains ("Green")) {
				canShootGreen = true;
				if (noAmmo) {
					currentAmmo = GREEN;
				}
			}
			if (other.tag.Contains ("Blue")) {
				canShootBlue = true;
				if (noAmmo) {
					currentAmmo = BLUE;
				}
			}
			Destroy (other.gameObject);
		}
	}
	
	void Shoot(GameObject ammo) {
		GameObject bullet = (GameObject) Instantiate (ammo, headTrans.position, headTrans.rotation);
		bullet.GetComponent<Rigidbody> ().AddForce (bullet.transform.forward * shootForce);
		Physics.IgnoreCollision (GetComponent<Collider> (), bullet.GetComponent<Collider> ());
	}
	
	void ResetCooldown () {
		firingCooldown = cooldown;
	}
	
	bool CanShoot () {
		return firingCooldown <= 0;
	}

	void Update () {
		// Movement
		Vector3 move = new Vector3 ();
		move.x = Input.GetAxis ("Horizontal");
		move.z = Input.GetAxis ("Vertical");
		move = transform.TransformDirection (move);
		move = move.normalized * moveSpeed;
		move.y = character.velocity.y;
		if (!character.isGrounded) {
			move.y -= gravity * Time.deltaTime;
		} else {
			if (Input.GetButton ("Jump")) {
				move.y = jumpForce;
			} else {
				move.y = 0;
			}
		}
		character.Move (move * Time.deltaTime);

		// Mouselook
		Vector2 mouse;
		if (Cursor.lockState == CursorLockMode.Locked) {
			mouse = new Vector2 (Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"));
		} else {
			mouse = new Vector2 (0, 0);
		}
		rotation.x += mouse.x * GameController.mouseSensitivity.x;
		rotation.y -= mouse.y * GameController.mouseSensitivity.y;
		rotation.y = Mathf.Clamp (rotation.y, -90, 90);
		bodyTrans.localEulerAngles = new Vector3 (bodyTrans.localEulerAngles.x, rotation.x, bodyTrans.localEulerAngles.z);
		headTrans.localEulerAngles = new Vector3 (rotation.y, headTrans.localEulerAngles.y, headTrans.localEulerAngles.z);

		// Flashlight
		if (Input.GetButton ("Flashlight") && Time.realtimeSinceStartup - flashlightToggled > 0.2) {
			flashlightOn = !flashlightOn;
			spotlight.SetActive(flashlightOn);
			Debug.Log ("Flashlight toggle! Flashlight status: " + spotlight.activeSelf);
			flashlightToggled = Time.realtimeSinceStartup;
		}

		// Scroll through ammo
		if (canShootYellow || canShootGreen || canShootBlue) {
			int mouseScrollWheel = (int) (Input.GetAxis ("Mouse ScrollWheel") * 10);
			currentAmmo -= mouseScrollWheel;
			while (currentAmmo > BLUE) {
				currentAmmo = YELLOW;
			}
			while (currentAmmo < YELLOW) {
				currentAmmo = BLUE;
			}
		}

		// Shooting
		if (!CanShoot ()) {
			firingCooldown -= Time.deltaTime;
		}
		if (Input.GetButton("Shoot") && CanShoot ()) {
			if (currentAmmo == YELLOW && canShootYellow) {
				Debug.Log("Firing yellow!");
				Shoot (yellowAmmo);
			}
			if (currentAmmo == GREEN && canShootGreen) {
				Debug.Log("Firing green!");
				Shoot (greenAmmo);
			}
			if (currentAmmo == BLUE && canShootBlue) {
				Debug.Log("Firing blue!");
				Shoot (blueAmmo);
			}
			ResetCooldown ();
		}

		// Update equip text
		switch (currentAmmo) {
		default:
			equipText.text = originalEquipText + "Off";
			break;
		case NO_AMMO:
			equipText.text = originalEquipText + "No ammo";
			break;
		case YELLOW:
			equipText.text = originalEquipText + "Yellow";
			break;
		case GREEN:
			equipText.text = originalEquipText + "Green";
			break;
		case BLUE:
			equipText.text = originalEquipText + "Blue";
			break;
		}
	}

}
