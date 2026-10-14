extends Control

@onready var country_label: Label = $VBoxContainer/CountryLabel
@onready var treasury_label: Label = $VBoxContainer/TreasuryLabel
@onready var approval_label: Label = $VBoxContainer/ApprovalLabel
@onready var ruling_party_label: Label = $VBoxContainer/RulingPartyLabel
@onready var election_label: Label = $VBoxContainer/ElectionLabel


func _ready() -> void:
	country_label.text = "Country: Republic of Walesky"
	treasury_label.text = "Treasury: R 1,250,000,000"
	approval_label.text = "Approval: 62"
	ruling_party_label.text = "Ruling party: Civic Union"
	election_label.text = "Election: No election"
