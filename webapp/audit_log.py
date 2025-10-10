"""
Audit Log System
Tracks all changes to the database with timestamp, IP, and change details
"""
from pathlib import Path
from datetime import datetime
import json
from typing import Dict, Any, Optional

AUDIT_LOG_FILE = Path(__file__).parent.parent / "audit_log.jsonl"

def log_action(
    action: str,
    product_name: Optional[str],
    ip_address: str,
    user_agent: str,
    details: Optional[Dict[str, Any]] = None
):
    """
    Log an action to the audit log
    
    Args:
        action: Type of action (create, update, delete, auto_populate, scan)
        product_name: Name of product affected (None for global actions)
        ip_address: IP address of requester
        user_agent: User agent string
        details: Additional details about the change
    """
    log_entry = {
        "timestamp": datetime.now().isoformat(),
        "action": action,
        "product": product_name,
        "ip": ip_address,
        "user_agent": user_agent,
        "details": details or {}
    }
    
    # Append to JSONL file (one JSON object per line)
    try:
        with open(AUDIT_LOG_FILE, 'a', encoding='utf-8') as f:
            f.write(json.dumps(log_entry, ensure_ascii=False) + '\n')
    except Exception as e:
        print(f"Warning: Failed to write audit log: {e}")

def get_recent_logs(limit: int = 100) -> list:
    """
    Get recent audit log entries
    
    Args:
        limit: Maximum number of entries to return
    
    Returns:
        List of log entries (most recent first)
    """
    if not AUDIT_LOG_FILE.exists():
        return []
    
    logs = []
    try:
        with open(AUDIT_LOG_FILE, 'r', encoding='utf-8') as f:
            for line in f:
                if line.strip():
                    logs.append(json.loads(line))
        
        # Return most recent first
        return logs[-limit:][::-1]
    except Exception as e:
        print(f"Warning: Failed to read audit log: {e}")
        return []

def get_product_history(product_name: str, limit: int = 50) -> list:
    """
    Get audit history for a specific product
    
    Args:
        product_name: Name of the product
        limit: Maximum number of entries to return
    
    Returns:
        List of log entries for this product (most recent first)
    """
    if not AUDIT_LOG_FILE.exists():
        return []
    
    product_logs = []
    try:
        with open(AUDIT_LOG_FILE, 'r', encoding='utf-8') as f:
            for line in f:
                if line.strip():
                    entry = json.loads(line)
                    if entry.get('product') == product_name:
                        product_logs.append(entry)
        
        # Return most recent first
        return product_logs[-limit:][::-1]
    except Exception as e:
        print(f"Warning: Failed to read audit log: {e}")
        return []
