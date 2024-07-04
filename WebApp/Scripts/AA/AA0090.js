Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/AA0090/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "藥學基本檔維護作業(TDM/各庫查詢)";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        id: 'P0',
        fieldLabel: '院內碼',
        allowBlank: true,
        labelWidth: 60,
        width: 240,
        //forceSelection: true,
        fieldCls: 'required',
        allowBlank: false,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0090/GetMMCodeCombo',

        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T1Query.getForm();
                if (r.get('P0') !== '') {
                    f.findField("P0").setValue(r.get('MMCODE'));
                    f.findField("P1").setValue(r.get('MMNAME_C'));
                    f.findField("P2").setValue(r.get('MMNAME_E'));
                    f.findField("P3").setValue(r.get('INSUORDERCODE'));
                    f.findField("P4").setValue(r.get('ORDERHOSPNAME'));
                    f.findField("P5").setValue(r.get('ORDEREASYNAME'));
                    f.findField("P6").setValue(r.get('SCIENTIFICNAME'));
                    f.findField("P7").setValue(r.get('CURECONSISTENCY'));
                    f.findField("P8").setValue(r.get('PEAR'));
                    f.findField("P9").setValue(r.get('TROUGH'));
                    f.findField("P10").setValue(r.get('DANGER'));
                    if (r.get('TDMFLAG') == 'Y') {
                        f.findField("P11").setValue(true);
                    }
                    else {
                        f.findField("P11").setValue(false);
                    }
                    f.findField("P12").setValue(r.get('TDMMEMO1'));
                    f.findField("P13").setValue(r.get('TDMMEMO2'));
                    f.findField("P14").setValue(r.get('TDMMEMO3'));
                    if (r.get('UDSERVICEFLAG') == 'Y') {
                        f.findField("P15").setValue(true);
                    }
                    else {
                        f.findField("P15").setValue(false);
                    }
                    if (r.get('UDPOWDERFLAG') == 'Y') {
                        f.findField("P16").setValue(true);
                    }
                    else {
                        f.findField("P16").setValue(false);
                    }
                    if (r.get('AIRDELIVERY') == 'Y') {
                        f.findField("P17").setValue(true);
                    }
                    else {
                        f.findField("P17").setValue(false);
                    }
                }
            }
        }
    });

    function setMmcode(args) {
        if (args.MMCODE !== '') {
            T1Query.getForm().findField("P0").setValue(args.MMCODE);
            T1Query.getForm().findField("P1").setValue(args.MMNAME_C);
            T1Query.getForm().findField("P2").setValue(args.MMNAME_E);
            T1Query.getForm().findField("P3").setValue(args.INSUORDERCODE);
            T1Query.getForm().findField("P4").setValue(args.ORDERHOSPNAME);
            T1Query.getForm().findField("P5").setValue(args.ORDEREASYNAME);
            T1Query.getForm().findField("P6").setValue(args.SCIENTIFICNAME);
            T1Query.getForm().findField("P7").setValue(args.CURECONSISTENCY);
            T1Query.getForm().findField("P8").setValue(args.PEAR);
            T1Query.getForm().findField("P9").setValue(args.TROUGH);
            T1Query.getForm().findField("P10").setValue(args.DANGER);
            if (args.TDMFLAG == 'Y') {
                T1Query.getForm().findField("P11").setValue(true);
            }
            else {
                T1Query.getForm().findField("P11").setValue(false);
            }
            T1Query.getForm().findField("P12").setValue(args.TDMMEMO1);
            T1Query.getForm().findField("P13").setValue(args.TDMMEMO2);
            T1Query.getForm().findField("P14").setValue(args.TDMMEMO3);
            if (args.UDSERVICEFLAG == 'Y') {
                T1Query.getForm().findField("P15").setValue(true);
            }
            else {
                T1Query.getForm().findField("P15").setValue(false);
            }
            if (args.UDPOWDERFLAG == 'Y') {
                T1Query.getForm().findField("P16").setValue(true);
            }
            else {
                T1Query.getForm().findField("P16").setValue(false);
            }
            if (args.AIRDELIVERY == 'Y') {
                T1Query.getForm().findField("P17").setValue(true);
            }
            else {
                T1Query.getForm().findField("P17").setValue(false);
            }
        }
    }

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                mmCodeCombo, {
                    xtype: 'button',
                    itemId: 'btnMmcode',
                    iconCls: 'TRASearch',
                    handler: function () {
                        var f = T1Query.getForm();
                        popMmcodeForm(viewport, '/api/AA0090/GetMmcode', { MMCODE: f.findField("P0").getValue() }, setMmcode);
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P1',
                    id: 'P1',
                    readOnly: true,
                    labelWidth: 70,
                    width: 330
                }, {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P2',
                    id: 'P2',
                    readOnly: true,
                    labelWidth: 70,
                    width: 350
                }, {
                    xtype: 'textfield',
                    fieldLabel: '健保碼',
                    name: 'P3',
                    id: 'P3',
                    readOnly: true,
                    labelWidth: 60,
                    width: 200
                }, {
                    xtype: 'textfield',
                    fieldLabel: '別名(院內名稱)',
                    name: 'P4',
                    id: 'P4',
                    readOnly: true,
                    labelWidth: 110,
                    width: 330
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            padding: '4 0 4 0',
            items: [{
                xtype: 'textfield',
                fieldLabel: '簡稱',
                name: 'P5',
                id: 'P5',
                readOnly: true,
                labelWidth: 60,
                width: 180
            }, {
                xtype: 'textfield',
                fieldLabel: '成份名稱',
                name: 'P6',
                id: 'P6',
                readOnly: true,
                labelWidth: 70,
                width: 250
            }, {
                xtype: 'textfield',
                fieldLabel: '合理治療濃度',
                name: 'P7',
                id: 'P7',
                readOnly: true,
                labelWidth: 95,
                width: 225
            }, {
                xtype: 'textfield',
                fieldLabel: '合理PEAK',
                name: 'P8',
                id: 'P8',
                readOnly: true,
                labelWidth: 75,
                width: 205
            }, {
                xtype: 'textfield',
                fieldLabel: '合理Trough',
                name: 'P9',
                id: 'P9',
                readOnly: true,
                labelWidth: 85,
                width: 215
            }, {
                xtype: 'textfield',
                fieldLabel: '危急值',
                name: 'P10',
                id: 'P10',
                readOnly: true,
                labelWidth: 60,
                width: 195
            }, {
                xtype: 'checkboxfield',
                fieldLabel: 'TDM藥品',
                name: 'P11',
                id: 'P11',
                readOnly: true,
                readOnly: true,
                labelWidth: 65
            }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '備註1',
                    name: 'P12',
                    id: 'P12',
                    readOnly: true,
                    labelWidth: 60,
                    width: 250
                }, {
                    xtype: 'textfield',
                    fieldLabel: '備註2',
                    name: 'P13',
                    id: 'P13',
                    readOnly: true,
                    labelWidth: 55,
                    width: 245
                }, {
                    xtype: 'textfield',
                    fieldLabel: '備註3',
                    name: 'P14',
                    id: 'P14',
                    readOnly: true,
                    labelWidth: 55,
                    width: 245
                }, {
                    xtype: 'checkboxfield',
                    fieldLabel: '使用自動調配機',
                    name: 'P15',
                    id: 'P15',
                    readOnly: true,
                    labelWidth: 105
                }, {
                    xtype: 'checkboxfield',
                    fieldLabel: 'UD磨粉',
                    name: 'P16',
                    id: 'P16',
                    readOnly: true,
                    labelWidth: 100
                }, {
                    xtype: 'checkboxfield',
                    fieldLabel: '可氣送',
                    name: 'P17',
                    id: 'P17',
                    readOnly: true,
                    labelWidth: 100,
                    width: 150
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        if (T1Query.getForm().isValid()) {
                            T1Load();
                        }
                        else {
                            T1Store.removeAll();
                            if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>院內碼</span>為必填');
                                msglabel(" <span style='color:red'>院內碼</span>為必填");
                            }
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0090', { // 定義於/Scripts/app/store/AA0090.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        },
        {
            text: "庫別代碼",
            dataIndex: 'WH_NO',
            width: 70
        }, {
            text: "庫別名",
            dataIndex: 'WH_NAME',
            width: 150
        }, {
            text: "最低庫存",
            dataIndex: 'LOW_QTY',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "安全存量天數",
            dataIndex: 'SAFE_DAY',
            width: 90,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "安全存量",
            dataIndex: 'SAFE_QTY',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "基準天數",
            dataIndex: 'OPER_DAY',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "基準量",
            dataIndex: 'OPER_QTY',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "各庫停用",
            dataIndex: 'CANCEL_ID',
            width: 70
        }, {
            text: "最小包裝",
            dataIndex: 'MIN_ORDQTY',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "儲位",
            dataIndex: 'STORE_LOC',
            width: 100
        }, {
            text: "庫存量",
            dataIndex: 'INV_QTY',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "上級庫",
            dataIndex: 'PWH_NO',
            width: 80
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec) {
                    msglabel("");
                }
            }
        }
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    T1Query.getForm().findField('P0').focus(); //讓游標停在P0這一格

});
