Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/BG0003/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "衛材非庫備常態申購查詢";
    var T1GetExcel = '/api/BG0003/Excel';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var ClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //預設UserWhno
    function setClass() {
        ClassStore.add({ VALUE: '1', TEXT: '超過十萬品項' });
        ClassStore.add({ VALUE: '2', TEXT: '超過十萬單位' });
        ClassStore.add({ VALUE: '3', TEXT: '尚未開單單位' });
    }

    //取得當月第一天及當天
    function setDate() {
        startdate = new Date();
        startdate.setDate(1);
        startdate = Ext.Date.format(startdate, "Ymd") - 19110000;

        enddate = new Date();
        enddate = Ext.Date.format(enddate, "Ymd") - 19110000;
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
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '單位申請日期',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true,
                    allowBlank: false, // 欄位是否為必填
                    fieldCls: 'required',
                    blankText: "請輸入申請日期(起)",
                    labelWidth: 90,
                    width: 200,
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'X/m/d');
                    }
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    allowBlank: false, // 欄位是否為必填
                    fieldCls: 'required',
                    blankText: "請輸入申請日期(迄)",
                    labelWidth: 25,
                    width: 135,
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'X/m/d');
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '分類',
                    store: ClassStore,
                    name: 'P2',
                    id: 'P2',
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    blankText: "請輸入分類",
                    autoSelect: true,
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    labelWidth: 50,
                    width: 200,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    iconCls: 'TRASearch',
                    handler: function () {
                        msglabel("");
                        if (T1Query.getForm().isValid()) {
                            if (T1Query.getForm().findField("P2").getValue() == 1) {
                                setClassGrid(true, false, false);
                            }
                            else if (T1Query.getForm().findField("P2").getValue() == 2) {
                                setClassGrid(false, true, false);
                            }
                            else if (T1Query.getForm().findField("P2").getValue() == 3) {
                                setClassGrid(false, false, true);
                            }
                            T1Load();
                        }
                        else {
                            T1Store.removeAll();
                            if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>申請日期(起)</span>為必填');
                                msglabel(" <span style='color:red'>申請日期(起)</span>為必填");
                            }
                            else if ((T1Query.getForm().findField("P1").getValue() == null) || (T1Query.getForm().findField("P1").getValue() == "")) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>申請日期(迄)</span>為必填');
                                msglabel(" <span style='color:red'>申請日期(迄)</span>為必填");
                            }
                            else if ((T1Query.getForm().findField("P2").getValue() == null) || (T1Query.getForm().findField("P2").getValue() == "")) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>分類</span>為必填');
                                msglabel(" <span style='color:red'>分類</span>為必填");
                            }
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    iconCls: 'TRAClear',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        T1Query.getForm().findField('P0').setValue(startdate);
                        T1Query.getForm().findField('P1').setValue(enddate);
                        T1Query.getForm().findField('P2').setValue('1');
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BG0003', { // 定義於/Scripts/app/store/BG0003.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
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
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', handler: function () {
                    if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>申請日期(起)</span>為必填');
                        msglabel(" <span style='color:red'>申請日期(起)</span>為必填");
                    }
                    else if ((T1Query.getForm().findField("P1").getValue() == null) || (T1Query.getForm().findField("P1").getValue() == "")) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>申請日期(迄)</span>為必填');
                        msglabel(" <span style='color:red'>申請日期(迄)</span>為必填");
                    }
                    else if ((T1Query.getForm().findField("P2").getValue() == null) || (T1Query.getForm().findField("P2").getValue() == "")) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>分類</span>為必填');
                        msglabel(" <span style='color:red'>分類</span>為必填");
                    }
                    else {
                        var p = new Array();
                        p.push({ name: 'FN', value: '衛材非庫備常態申購查詢.xls' });
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                        p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                        p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                        PostForm(T1GetExcel, p);
                        msglabel('匯出完成');
                    }
                }
            }
        ]
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
            xtype: 'rownumberer', // 0
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        },
        {
            text: "年月", // 1
            dataIndex: 'YYYMM',
            width: 60
        }, {
            text: "院內碼", // 2
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "英文品名", // 3
            dataIndex: 'MMNAME_E',
            width: 300
        }, {
            text: "中文品名", // 4
            dataIndex: 'MMNAME_C',
            width: 250
        }, {
            text: "計量單位", // 5
            dataIndex: 'BASE_UNIT',
            width: 60
        }, {
            text: "單價", // 6
            dataIndex: 'UPRICE',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "廠商碼", // 7
            dataIndex: 'M_AGENNO',
            width: 60
        }, {
            text: "廠商名稱", // 8
            dataIndex: 'AGEN_NAMEC',
            width: 150
        }, {
            text: "物料類別", // 9
            dataIndex: 'MAT_CLASS',
            width: 60
        }, {
            text: "數量", // 10
            dataIndex: 'APPQTY',
            width: 70,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "預估申購金額", // 11
            dataIndex: 'ESTPAY',
            width: 95,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        },
        //////
        {
            text: "年月", // 12
            dataIndex: 'YYYMM_2',
            width: 60
        }, {
            text: "責任中心", // 13
            dataIndex: 'INID',
            width: 170
        }, {
            text: "申請單號", // 14
            dataIndex: 'DOCNO',
            width: 170
        }, {
            text: "院內碼", // 15
            dataIndex: 'MMCODE_2',
            width: 80
        }, {
            text: "英文品名", // 16
            dataIndex: 'MMNAME_E_2',
            width: 300
        }, {
            text: "中文品名", // 17
            dataIndex: 'MMNAME_C_2',
            width: 250
        }, {
            text: "計量單位", // 18
            dataIndex: 'BASE_UNIT_2',
            width: 60
        }, {
            text: "單價", // 19
            dataIndex: 'UPRICE_2',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "數量", // 20
            dataIndex: 'APPQTY_2',
            width: 70,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "預估申購金額", // 21
            dataIndex: 'ESTPAY_2',
            width: 95,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        },
        //////
        {
            text: "責任中心", // 22
            dataIndex: 'APPDEPT',
            width: 90
        }, {
            text: "責任中心名稱", // 23
            dataIndex: 'INID_NAME',
            width: 200
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

    //控制Grid顯示
    function setClassGrid(T1, T2, T3) {
        T1 ? setT1Grid(true) : setT1Grid(false);
        T2 ? setT2Grid(true) : setT2Grid(false);
        T3 ? setT3Grid(true) : setT3Grid(false);
    }

    function setT1Grid(T1) {
        T1Grid.columns[1].setVisible(T1);
        T1Grid.columns[2].setVisible(T1);
        T1Grid.columns[3].setVisible(T1);
        T1Grid.columns[4].setVisible(T1);
        T1Grid.columns[5].setVisible(T1);
        T1Grid.columns[6].setVisible(T1);
        T1Grid.columns[7].setVisible(T1);
        T1Grid.columns[8].setVisible(T1);
        T1Grid.columns[9].setVisible(T1);
        T1Grid.columns[10].setVisible(T1);
        T1Grid.columns[11].setVisible(T1);
    }

    function setT2Grid(T2) {
        T1Grid.columns[12].setVisible(T2);
        T1Grid.columns[13].setVisible(T2);
        T1Grid.columns[14].setVisible(T2);
        T1Grid.columns[15].setVisible(T2);
        T1Grid.columns[16].setVisible(T2);
        T1Grid.columns[17].setVisible(T2);
        T1Grid.columns[18].setVisible(T2);
        T1Grid.columns[19].setVisible(T2);
        T1Grid.columns[20].setVisible(T2);
        T1Grid.columns[21].setVisible(T2);
    }

    function setT3Grid(T3) {
        T1Grid.columns[22].setVisible(T3);
        T1Grid.columns[23].setVisible(T3);
    }

    setClass();
    setDate();
    T1Query.getForm().findField('P0').focus(); //讓游標停在P0這一格
    T1Query.getForm().findField('P0').setValue(startdate);
    T1Query.getForm().findField('P1').setValue(enddate);
    T1Query.getForm().findField('P2').setValue('1');
    setClassGrid(true, false, false);
});
