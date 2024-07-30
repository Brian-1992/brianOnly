Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "藥品基本檔維護作業";
    var MmcodeComboGet = '/api/AA0038/GetMmcodeCombo';
    var MatclassComboGet = '/api/AA0038/GetMatclassCombo';
    var IsadminGet = '/api/AA0038/ChkIsAdmg';
    var T1GetExcel = '/api/AA0058/Excel';

    var T1Rec = 0;
    var T1LastRec = null;
    var isAdmin = 'N';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 院內碼
    var mmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    // 物品分類代碼
    var matclassQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    function setComboData() {
        //Ext.Ajax.request({
        //    url: MmcodeComboGet,
        //    method: reqVal_p,
        //    success: function (response) {
        //        var data = Ext.decode(response.responseText);
        //        if (data.success) {
        //            var tb_data = data.etts;
        //            if (tb_data.length > 0) {
        //                mmcodeQueryStore.add({ VALUE: '', COMBITEM: '' });
        //                for (var i = 0; i < tb_data.length; i++) {
        //                    mmcodeQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
        //                }
        //            }
        //        }
        //    },
        //    failure: function (response, options) {

        //    }
        //});

        Ext.Ajax.request({
            url: MatclassComboGet,
            method: reqVal_p,
            params: { p0: 'E' },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        matclassQueryStore.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            matclassQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                        T1Query.getForm().findField('P3').setValue('01');
                    }
                }
            },
            failure: function (response, options) {

            }
        });

    }
    setComboData();

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
                    store: mmcodeQueryStore,
                    id: 'P0',
                    name: 'P0',
                    enforceMaxLength: true,
                    maxLength: 13,
                    fieldLabel: '院內碼',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4'
                }, {
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
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            hidden: true,
            items: [
                {
                    xtype: 'combo',
                    store: matclassQueryStore,
                    id: 'P3',
                    name: 'P3',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    fieldLabel: '物料分類代碼',
                    queryMode: 'local',
                    autoSelect: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: false,
                    listConfig: { width: 180 },
                    anyMatch: true,
                    padding: '0 4 0 4'
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.MiMastMaintain', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0058/GetAll',
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
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records) {
                for (var i = 0; i < records.length; i++)
                {
                    if (records[i].data.E_CODATE != null) {
                        var e_codate = records[i].data['E_CODATE'].toString();
                        if (e_codate.substring(0, 21) == 'Mon Jan 01 1 00:00:00' || e_codate.substring(0, 24) == 'Mon Jan 01 0001 00:00:00')
                            records[i].data['E_CODATE'] = ''; // 若資料庫值為null,在這邊另外做清除
                    }
                }
                store.setData(records);
                T1Grid.down('#t1export').setDisabled(records.length == 0);
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
        plain: true,
        buttons: [
            {
                text: '新增',
                itemId: 'btnAdd',
                hidden: true,
                handler: function () {
                    msglabel('訊息區:');
                    popWinForm('I', '');
                }
            },
            {
                itemId: 'edit',
                text: '修改',
                disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    popWinForm('U', T1LastRec.data['MMCODE']);
                }
            },
            {
                itemId: 't1export', text: '匯出', disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: T1Name + '.xlsx' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }
        ]
    });

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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            renderer: function (val, meta, record) {
                if (val != null)
                    return '<a href=javascript:popWinForm("V","' + record.data['MMCODE'] + '") >' + val + '</a>';
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 170
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 170
        }, {
            text: "物料分類代碼",
            dataIndex: 'MAT_CLASS',
            width: 110
        }, {
            text: "計量單位代碼",
            dataIndex: 'BASE_UNIT',
            width: 100
        }, {
            text: "廠商供應狀態",
            dataIndex: 'E_SUPSTATUS',
            width: 90
        }, {
            text: "原製造商",
            dataIndex: 'E_MANUFACT',
            width: 90
        }, {
            text: "是否公藥",
            dataIndex: 'E_IFPUBLIC',
            width: 70
        }, {
            text: "扣庫規則分類",
            dataIndex: 'E_STOCKTYPE',
            width: 90
        }, {
            text: "劑量",
            dataIndex: 'E_SPECNUNIT',
            width: 100
        }, {
            text: "成份",
            dataIndex: 'E_COMPUNIT',
            width: 100
        }, {
            text: "軍聯項次年號",
            dataIndex: 'E_YRARMYNO',
            width: 110
        }, {
            text: "軍聯項次號",
            dataIndex: 'E_ITEMARMYNO',
            width: 70
        }, {
            text: "軍聯項次分類",
            dataIndex: 'E_GPARMYNO',
            width: 100
        }, {
            text: "軍聯項次組別",
            dataIndex: 'E_CLFARMYNO',
            width: 100
        }, {
            xtype: 'datecolumn',
            text: "合約效期",
            dataIndex: 'E_CODATE',
            format: 'Xmd',
            width: 80
        }, {
            text: "藥品單複方",
            dataIndex: 'E_PRESCRIPTYPE',
            width: 80
        }, {
            text: "用藥類別",
            dataIndex: 'E_DRUGCLASS',
            width: 80
        }, {
            text: "藥品性質",
            dataIndex: 'E_DRUGCLASSIFY',
            width: 80
        }, {
            text: "藥品劑型",
            dataIndex: 'E_DRUGFORM',
            width: 80
        }, {
            text: "藥委會註記",
            dataIndex: 'E_COMITMEMO',
            width: 200
        }, {
            text: "藥委會品項",
            dataIndex: 'E_COMITCODE',
            width: 80
        }, {
            text: "是否盤點品項",
            dataIndex: 'E_INVFLAG',
            width: 100
        }, {
            text: "藥品採購案別",
            dataIndex: 'E_PURTYPE',
            width: 120
        }, {
            text: "來源代碼",
            dataIndex: 'E_SOURCECODE',
            width: 80
        }, {
            text: "藥品請領類別",
            dataIndex: 'E_DRUGAPLTYPE',
            width: 80
        }, {
            text: "軍品院內碼",
            dataIndex: 'E_ARMYORDCODE',
            width: 80
        }, {
            text: "母藥註記",
            dataIndex: 'E_PARCODE',
            width: 80
        }, {
            text: "母藥院內碼",
            dataIndex: 'E_PARORDCODE',
            width: 80
        }, {
            text: "子藥轉換量",
            dataIndex: 'E_SONTRANSQTY',
            align: 'right',
            style: 'text-align:left',
            width: 90
        }, {
            text: "批號效期註記",
            dataIndex: 'WEXP_ID',
            width: 100
        }, {
            text: "儲位記錄註記",
            dataIndex: 'WLOC_ID',
            width: 120
        }, {
            text: "管制用藥",
            dataIndex: 'E_RESTRICTCODE',
            width: 130
        }, {
            text: "停用碼",
            dataIndex: 'E_ORDERDCFLAG',
            width: 80
        }, {
            text: "高價用藥",
            dataIndex: 'E_HIGHPRICEFLAG',
            width: 80
        }, {
            text: "合理回流藥",
            dataIndex: 'E_RETURNDRUGFLAG',
            width: 100
        }, {
            text: "研究用藥",
            dataIndex: 'E_RESEARCHDRUGFLAG',
            width: 80
        }, {
            text: "疫苗",
            dataIndex: 'E_VACCINE',
            width: 70
        }, {
            text: "服用藥別",
            dataIndex: 'E_TAKEKIND',
            width: 80
        }, {
            text: "是否作廢",
            dataIndex: 'CANCEL_ID',
            width: 80
        }, {
            text: "院內給藥途徑(部位)代碼",
            dataIndex: 'E_PATHNO',
            width: 150
        }, {
            text: "醫囑單位",
            dataIndex: 'E_ORDERUNIT',
            width: 80
        }, {
            text: "門診給藥頻率",
            dataIndex: 'E_FREQNOO',
            width: 130
        }, {
            text: "住院給藥頻率",
            dataIndex: 'E_FREQNOI',
            width: 130
        }, {
            text: "合約碼",
            dataIndex: 'CONTRACNO',
            width: 200
        }, {
            text: "最小單價",
            dataIndex: 'UPRICE',
            align: 'right',
            style: 'text-align:left',
            width: 100
        }, {
            text: "合約單價",
            dataIndex: 'M_CONTPRICE',
            align: 'right',
            style: 'text-align:left',
            width: 120
        }, {
            text: "優惠合約單價",
            dataIndex: 'DISC_CPRICE',
            align: 'right',
            style: 'text-align:left',
            width: 120
        }, {
            text: "優惠最小單價",
            dataIndex: 'DISC_UPRICE',
            align: 'right',
            style: 'text-align:left',
            width: 120
        }, {
            text: "廠商包裝轉換率",
            dataIndex: 'EXCH_RATIO',
            align: 'right',
            style: 'text-align:left',
            width: 120
        }, {
            text: "院內簡稱",
            dataIndex: 'EASYNAME',
            width: 120
        }, {
            text: "作廢備註",
            dataIndex: 'CANCEL_NOTE',
            width: 80
        }, {
            text: "健保給付價",
            dataIndex: 'NHI_PRICE',
            align: 'right',
            style: 'text-align:left',
            width: 100
        }, {
            text: "自費價",
            dataIndex: 'HOSP_PRICE',
            align: 'right',
            style: 'text-align:left',
            width: 100
        }, {
            text: "生效起日",
            dataIndex: 'BEGINDATE',
            width: 80
        }, {
            text: "生效訖日",
            dataIndex: 'ENDDATE',
            width: 80
        }, {
            text: "住院扣庫轉換量",
            dataIndex: 'E_STOCKTRANSQTYI',
            align: 'right',
            style: 'text-align:left',
            width: 110
        }, {
            text: "成份名稱",
            dataIndex: 'E_SCIENTIFICNAME',
            width: 110
        }, {
            text: "單次訂購達優惠數量折讓意願",
            dataIndex: 'ISWILLING',
            width: 190
        }, {
            text: "單次採購優惠數量",
            dataIndex: 'DISCOUNT_QTY',
            width: 130
        }, {
            text: "單次訂購達優惠數量成本價",
            dataIndex: 'DISC_COST_UPRICE',
            width: 190
        }, {
            text: "健保碼",
            dataIndex: 'M_NHIKEY',
            width: 110
        },
        //{
        //        text: "標案來源",
        //    dataIndex: 'SELF_BID_SOURCE',
        //    width: 110
        //},
        {
                text: "合約案號",
            dataIndex: 'SELF_CONTRACT_NO',
            width: 110
        }, {
                text: "採購上限金額",
            dataIndex: 'SELF_PUR_UPPER_LIMIT',
            width: 110
        }, {
                text: "合約生效起日",
            dataIndex: 'SELF_CONT_BDATE',
            width: 110
        }, {
                text: "合約生效迄日",
            dataIndex: 'SELF_CONT_EDATE',
            width: 110
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                T1Grid.down('#edit').setDisabled(T1Rec === 0);
            }
        }
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            T1Query.getForm().reset();
                            T1Query.getForm().findField('P0').setValue(v.AGEN_NO);
                            T1Load();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            break;
                    }
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    // 供彈跳視窗內的呼叫以關閉彈跳視窗
    closeWin = function (rtnMmcode) {
        callableWin.destroy();
        callableWin = null;
        T1Query.getForm().reset();
        T1Query.getForm().findField('P0').setValue(rtnMmcode);
        T1Tool.doRefresh();
    }

    var callableWin = null;
    popWinForm = function (vType, strMmcode) {
        var strUrl = "AA0058_Form?strVtype=" + vType + "&strMmcode=" + strMmcode + "&strIsadm=" + isAdmin;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, '藥品基本檔詳細資料', viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }

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

    function chkIsAdmin() {
        Ext.Ajax.request({
            url: IsadminGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'Y') {
                        isAdmin = 'Y';
                        T1Tool.down('#btnAdd').setVisible(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    chkIsAdmin();

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();

    T1Query.getForm().findField('P3').setDisabled(true);
});
