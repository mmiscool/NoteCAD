﻿using System.Collections.Generic;
using System.Xml;
using System;
using UnityEngine;

public interface ICADObject {
	IdPath id { get; }
}

public abstract class CADObject : ICADObject {
	public abstract Id guid { get; }
	public abstract ICADObject GetChild(Id guid);
	public abstract CADObject parentObject { get; }

	public IdPath id {
		get {
			return GetRelativePath(null);
		}
	}

	public IdPath GetRelativePath(CADObject from) {
		var result = new IdPath();
		var p = this;
		while(p != null) {
			if(p == from) return result;
			if(p.guid == Id.Null) return result;
			result.path.Add(p.guid);
			p = p.parentObject;
		}
		return result;
	}

	public ICADObject GetObjectById(IdPath id) {
		var i = -1;
		var p = this;
		while(true) {
			if(p.guid == Id.Null) {
				i = id.path.Count;
				break;
			}
			//i = id.path.FindLastIndex(g => g == p.guid);
			//if(i != -1) break;
			p = p.parentObject;
			if(p == null) return null; 
		}
		ICADObject r = p;
		while(i > 0) {
			i--;
			var co = r as CADObject;
			if(co == null) return null;
			r = co.GetChild(id.path[i]);
			if(p == null) return null;
		}
		return r;
	}
}

public abstract class SketchObject : CADObject, ICADObject {

	Sketch sk;
	public Sketch sketch { get { return sk; } }
	public bool isDestroyed { get; private set; }

	Id guid_;
	public override Id guid { get { return guid_; } }

	public override CADObject parentObject {
		get {
			return sketch;
		}
	}

	public override ICADObject GetChild(Id guid) {
		return null;
	}

	public SketchObject(Sketch sketch) {
		sk = sketch;
		guid_ = sketch.idGenerator.New();
	}
	protected virtual GameObject gameObject { get { return null; } }

	public virtual IEnumerable<Param> parameters { get { yield break; } }
	public virtual IEnumerable<Exp> equations { get { yield break; } }

	protected virtual void OnDrag(Vector3 delta) { }

	public void Drag(Vector3 delta) {
		OnDrag(delta);
	}

	Color oldColor;

	bool hovered;
	public bool isHovered {
		get {
			return hovered;
		}
		set {
			if(value == hovered) return;
			hovered = value;
			if(gameObject == null) return;
			var r = gameObject.GetComponent<Renderer>();
			var t = gameObject.GetComponent<UnityEngine.UI.Text>();
			if(isError) return;
			if(!hovered) {
				if(r != null) {
					r.material.color = oldColor;
					r.material.renderQueue--;
				} else if(t != null) {
					t.color = oldColor;
				}
			} else {
				if(r != null) {
					oldColor = r.material.color;
					r.material.color = Color.yellow;
					r.material.renderQueue++;
				} else if(t != null) {
					oldColor = t.color;
					t.color = Color.yellow;
				}
			}
		}
	}

	bool error;
	public bool isError {
		get {
			return error;
		}
		set {
			if(value == error) return;
			if(value) {
				isHovered = false;
			}
			error = value;
			if(gameObject == null) return;
			var r = gameObject.GetComponent<Renderer>();
			var t = gameObject.GetComponent<UnityEngine.UI.Text>();
			if(!error) {
				if(r != null) {
					r.material.color = oldColor;
					r.material.renderQueue--;
				} else if(t != null) {
					t.color = oldColor;
				}
			} else {
				if(r != null) {
					oldColor = r.material.color;
					r.material.color = Color.red;
					r.material.renderQueue++;
				} else if(t != null) {
					oldColor = t.color;
					t.color = Color.red;
				}
			}
		}
	}

	bool selectable = true;
	public bool isSelectable {
		get {
			return selectable;
		}
		set {
			if(value == selectable) return;
			selectable = value;
			if(gameObject == null) return;
			var c = gameObject.GetComponent<Collider>();
			if(c == null) return;
			c.enabled = selectable;
		}
	}

	public virtual void Destroy() {
		if(isDestroyed) return;
		isDestroyed = true;
		sketch.Remove(this);
		OnDestroy();
	}

	protected virtual void OnDestroy() {

	}

	public virtual void Write(XmlTextWriter xml) {
		xml.WriteAttributeString("id", guid.ToString());
		OnWrite(xml);
	}

	protected virtual void OnWrite(XmlTextWriter xml) {

	}

	public virtual void Read(XmlNode xml) {

		guid_ = sketch.idGenerator.Create(xml.Attributes["id"].Value);
		OnRead(xml);
	}

	protected virtual void OnRead(XmlNode xml)  {
	
	}

	public void Draw(LineCanvas canvas) {
		OnDraw(canvas);
	}

	public virtual bool IsChanged() {
		return OnIsChanged();
	}

	protected virtual bool OnIsChanged() {
		return false;
	}

	protected virtual void OnDraw(LineCanvas canvas) {
		
	}

	public double Select(Vector3 mouse, Camera camera, Matrix4x4 tf) {
		return OnSelect(mouse, camera, tf);
	}

	protected virtual double OnSelect(Vector3 mouse, Camera camera, Matrix4x4 tf) {
		return -1.0;
	}

}
