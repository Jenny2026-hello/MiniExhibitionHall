# MiniExhibitionHall - 太空探索虚拟展览馆

> 基于 Unity 3D 的第一人称沉浸式虚拟展览馆 | 虚拟现实课程 Bonus 项目

## 项目概述

太空探索主题的虚拟展览馆，使用 Unity 引擎构建。用户可以自由走动浏览展品、靠近查看详情、拾取物品旋转观察，体验完整的虚拟太空展览。

## 展览内容

| 展区 | 展品 |
|------|------|
| 火箭技术区 | 长征火箭模型、人造卫星模型 |
| 行星探索区 | 火星模型、土星模型 |
| 深空未来区 | 国际空间站模型、深空探测器模型 |

## 技术栈

| 类别 | 工具 |
|------|------|
| 游戏引擎 | Unity 2021.3 LTS+ |
| 编程语言 | C# |
| 目标平台 | Windows / Mac (PC) |

## 交互方式

| 操作 | 功能 |
|------|------|
| WASD | 前后左右移动 |
| Shift + WASD | 奔跑 |
| 鼠标移动 | 视角旋转 |
| E | 查看展品 / 拾取展品 |
| 鼠标左键拖拽 | 旋转手中展品 |
| 鼠标滚轮 | 缩放手中展品 |
| ESC | 暂停菜单 |

## 快速开始

1. 用 Unity Hub 创建新 3D 项目（Unity 2021.3 LTS+）
2. 将 `Assets/` 下所有内容复制到项目 Assets 目录
3. 在 Unity 编辑器中点击 **Tools → 搭建太空探索展览馆**
4. 点击 **🚀 一键搭建太空展览馆**
5. 点击 Play 开始体验

## 项目结构

```
MiniExhibitionHall/
├── Assets/
│   ├── Scripts/
│   │   ├── Editor/
│   │   │   └── SpaceExhibitionBuilder.cs   # 一键场景搭建工具
│   │   ├── Player/
│   │   │   └── FirstPersonController.cs    # 第一人称控制器
│   │   ├── Exhibition/
│   │   │   ├── ExhibitBase.cs              # 展品基类
│   │   │   ├── ExhibitPickup.cs            # 可拾取展品
│   │   │   └── ZoneTrigger.cs              # 区域触发器
│   │   ├── UI/
│   │   │   └── Billboard.cs                # 始终面向相机
│   │   ├── Audio/
│   │   │   └── AudioManager.cs             # 音频管理器
│   │   └── GameManager.cs                  # 全局管理器
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Materials/
│   └── Audio/
└── Docs/
    ├── 开发文档.md
    ├── 场景搭建指南.md
    ├── 项目报告模板.md
    └── 太空探索展搭建指南.md
```

## 文档

详细开发文档和场景搭建指南请参见 `Docs/` 目录。
