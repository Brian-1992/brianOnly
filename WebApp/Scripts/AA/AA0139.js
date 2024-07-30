Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "國軍藥品聯標決標品項維護";
    var T1GetExcel = '/api/AA0139/ExcelT1';
    var T2GetExcel = '/api/AA0139/ExcelT2';

    var T1Rec = 0;
    var T1LastRec = null;
    var isAdmin = 'N';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    id: 'P0',
                    name: 'P0',
                    enforceMaxLength: true,
                    maxLength: 3,
                    maskRe: /[0-9]/,
                    labelWidth: 110,
                    width: 160,
                    fieldLabel: '聯標生效(民國年)',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    enforceMaxLength: true,
                    maxLength: 3,
                    maskRe: /[0-9]/,
                    labelWidth: 10,
                    width: 60,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '投標項次',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            padding: '4 4 0 16',
            items: [
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '匯入總表',
                    handler: function () {
                        showWin1();
                    }
                }, {
                    xtype: 'button',
                    text: '顯示異動',
                    handler: function () {
                        showWin2();
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0139', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0139/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records) {
                store.setData(records);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var colTmp = [{
        xtype: 'rownumberer'
    }, {
        text: "國軍藥品聯標決標品項異動記錄流水號",
        itemId: 'COL_JBIDSEQ',
        dataIndex: 'JBIDSEQ',
        width: 100
    }, {
        text: "異動代碼",
        itemId: 'COL_TRANSCODE',
        dataIndex: 'TRANSCODE',
        width: 100
    }, {
        text: "聯標生效起年",
        dataIndex: 'JBID_STYR',
        width: 100
    }, {
        text: "聯標生效迄年",
        dataIndex: 'JBID_EDYR',
        width: 100
    }, {
        text: "投標項次",
        dataIndex: 'BID_NO',
        width: 70
    }, {
        text: "招標成分",
        dataIndex: 'INGR',
        width: 110
    }, {
        text: "成分含量",
        dataIndex: 'INGR_CONTENT',
        width: 80
    }, {
        text: "規格量",
        dataIndex: 'SPEC',
        width: 90
    }, {
        text: "劑型",
        dataIndex: 'DOSAGE_FORM',
        width: 90
    }, {
        text: "英文品名",
        dataIndex: 'MMNAME_E',
        width: 150
    }, {
        text: "中文品名",
        dataIndex: 'MMNAME_C',
        width: 150
    }, {
        text: "包裝",
        dataIndex: 'PACKQTY',
        width: 100
    }, {
        text: "原廠牌",
        dataIndex: 'ORIG_BRAND',
        width: 100
    }, {
        text: "許可證字號",
        dataIndex: 'LICENSE_NO',
        width: 110
    }, {
        text: "單次採購達優惠數量折讓意願",
        dataIndex: 'ISWILLING',
        width: 180
    }, {
        text: "單次採購優惠數量",
        dataIndex: 'DISCOUNT_QTY',
        width: 150
    }, {
        text: "健保代碼",
        dataIndex: 'INSU_CODE',
        width: 100
    }, {
        text: "健保價(健保品項)/上月預算單價(非健保品項)",
        dataIndex: 'INSU_RATIO',
        width: 230
    }, {
        text: "決標契約單價",
        dataIndex: 'K_UPRICE',
        width: 110
    }, {
        text: "決標成本單價",
        dataIndex: 'COST_UPRICE',
        width: 110
    }, {
        text: "單次訂購達優惠數量成本價",
        dataIndex: 'DISC_COST_UPRICE',
        width: 180
    }, {
        text: "廠商統編",
        dataIndex: 'UNIFORM_NO',
        width: 100
    }, {
        text: "廠商名稱",
        dataIndex: 'AGEN_NAME',
        width: 120
    }, {
        text: "修改年月日",
        dataIndex: 'UPDATE_YMD',
        width: 150
    }, {
        text: "院內碼",
        itemId: 'COL_MMCODELIST',
        dataIndex: 'MMCODELIST',
        width: 100
    }, {
        header: "",
        flex: 1
    }];

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
            }],
        columns: colTmp,
    });

    // 流水號和異動代碼只在[顯示異動]顯示
    T1Grid.down('#COL_JBIDSEQ').setVisible(false);
    T1Grid.down('#COL_TRANSCODE').setVisible(false);
    T1Grid.down('#COL_MMCODELIST').setVisible(false);

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
        }
        ]
    });

    // 匯入總表彈跳視窗
    var win1ActWidth = 300;
    var win1ActHeight = 150;

    var Win1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'panel',
                id: 'PanelWin1P1',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'textfield',
                    id: 'Win1P0',
                    name: 'Win1P0',
                    enforceMaxLength: true,
                    maxLength: 3,
                    maskRe: /[0-9]/,
                    labelWidth: 110,
                    width: 160,
                    fieldLabel: '聯標生效(民國年)',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '至',
                    name: 'Win1P1',
                    id: 'Win1P1',
                    labelSeparator: '',
                    enforceMaxLength: true,
                    maxLength: 3,
                    maskRe: /[0-9]/,
                    labelWidth: 10,
                    width: 60,
                    padding: '0 4 0 4'
                }]
            },
            {
                xtype: 'panel',
                id: 'PanelWin1P2',
                border: false,
                layout: 'hbox',
                padding: '4 4 4 0',
                items: [{
                    xtype: 'textfield',
                    fieldLabel: '修改年月日(例如1100401)',
                    name: 'Win1P2',
                    id: 'Win1P2',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 150,
                    width: 220,
                    maskRe: /[0-9]/,
                    padding: '0 4 0 4'
                }]
            },
            {
                xtype: 'panel',
                id: 'PanelWin1P3',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'filefield',
                    name: 'Win1send',
                    id: 'Win1send',
                    buttonOnly: true,
                    buttonText: '確定',
                    width: 40,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            var files = event.target.files;
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            if (!/^(xls|xlsx)$/.test(ext)) {
                                Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                                Ext.getCmp('Win1send').fileInputEl.dom.value = '';
                                msglabel("請選擇xlsx或xls檔案！");
                            }
                            else {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                myMask.show();
                                var win1P0 = Win1Query.getForm().findField('Win1P0').getValue();
                                var win1P1 = Win1Query.getForm().findField('Win1P1').getValue();
                                var win1P2 = Win1Query.getForm().findField('Win1P2').getValue();
                                if (win1P0 != "" && win1P1 != "" && win1P2 != "")
                                {
                                    var formData = new FormData();
                                    formData.append("file", f);
                                    formData.append("win1P0", win1P0);
                                    formData.append("win1P1", win1P1);
                                    formData.append("win1P2", win1P2);
                                    var ajaxRequest = $.ajax({
                                        type: "POST",
                                        url: "/api/AA0139/SendExcel",
                                        data: formData,
                                        processData: false,
                                        //必須false才會自動加上正確的Content-Type
                                        contentType: false,
                                        success: function (data, textStatus, jqXHR) {
                                            if (!data.success) {
                                                Ext.MessageBox.alert("提示", data.msg);
                                                msglabel("");
                                            }
                                            else {
                                                if (data.msg == "True") {
                                                    msglabel("檔案匯入成功");
                                                    Ext.MessageBox.alert("提示", "匯入完成!");
                                                    hideWin1();
                                                };
                                                if (data.msg == "False") {
                                                    Ext.MessageBox.alert("提示", "匯入失敗");
                                                };
                                            }
                                            Ext.getCmp('Win1send').fileInputEl.dom.value = '';
                                            myMask.hide();
                                        },
                                        error: function (jqXHR, textStatus, errorThrown) {
                                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                                            Ext.getCmp('Win1send').fileInputEl.dom.value = '';
                                            myMask.hide();
                                        }
                                    });
                                }
                                else
                                {
                                    Ext.Msg.alert('訊息', '聯標生效起迄年、修改年月日為必填!');
                                    Ext.getCmp('Win1send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                }
                            }
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '取消',
                    id: 'Win1Close',
                    name: 'Win1Close',
                    handler: function () {
                        hideWin1();
                    }
                }]
            }
        ]
    });

    function showWin1() {
        if (win1.hidden) {
            win1.show();
        }
    }
    function hideWin1() {
        if (!win1.hidden) {
            win1.hide();
            Win1Cleanup();
        }
    }
    function Win1Cleanup() {
        Win1Query.getForm().reset();
        msglabel('訊息區:');
    }

    var win1;
    if (!win1) {
        win1 = Ext.widget('window', {
            title: '匯入總表',
            closeAction: 'hide',
            width: win1ActWidth,
            height: win1ActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [Win1Query],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - win1ActWidth > 0) ? win1ActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - win1ActHeight > 0) ? win1ActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    win1ActWidth = width;
                    win1ActHeight = height;
                }
            }
        });
    }

    // 顯示異動彈跳視窗
    var win2ActWidth = viewport.width - 10;
    var win2ActHeight = viewport.height - 10;

    // 查詢欄位
    var Win2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelWin2P1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    id: 'Win2P0',
                    name: 'Win2P0',
                    enforceMaxLength: true,
                    maxLength: 5,
                    maskRe: /[0-9]/,
                    labelWidth: 120,
                    width: 190,
                    fieldLabel: '修改年月(例如11004)',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        Win2Load();
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('Win2P0').focus();
                        msglabel('');
                    }
                }
            ]
        }]
    });

    function showWin2() {
        if (win2.hidden) {
            win2.show();
        }
    }
    function hideWin2() {
        if (!win2.hidden) {
            win2.hide();
            Win2Cleanup();
        }
    }
    function Win2Cleanup() {
        Win2Query.getForm().reset();
        msglabel('訊息區:');
    }  

    var Win2Store = Ext.create('WEBAPP.store.AA.AA0139', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0139/GetWin2All_1',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: Win2Query.getForm().findField('Win2P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records) {
                store.setData(records);
            }
        }
    });
    function Win2Load() {
        Win2Tool.moveFirst();
    }

    var Win2Tool = Ext.create('Ext.PagingToolbar', {
        store: Win2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'Win2Export1', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: Win2Query.getForm().findField('Win2P0').getValue() });
                    PostForm(T1GetExcel, p);
                }
            }, {
                itemId: 'Win2Export2', text: '匯出(多筆)', handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: Win2Query.getForm().findField('Win2P0').getValue() });
                    PostForm(T2GetExcel, p);
                }
            }
        ]
    });

    var Win2Grid = Ext.create('Ext.grid.Panel', {
        store: Win2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [Win2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [Win2Tool]
        }],
        columns: colTmp
    });

    var win2;
    if (!win2) {
        win2 = Ext.widget('window', {
            title: '顯示異動',
            closeAction: 'hide',
            width: win2ActWidth,
            height: win2ActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [Win2Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - win2ActWidth > 0) ? win2ActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - win2ActHeight > 0) ? win2ActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    win2ActWidth = width;
                    win2ActHeight = height;
                }
            }
        });
    }
});
