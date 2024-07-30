Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "衛材基本檔維護作業"; // 含能設,通信
    var MmcodeComboGet = '/api/AA0038/GetMmcodeCombo';
    var MatclassComboGet = '/api/AA0038/GetMatclassCombo';
    var IsadminGet = '/api/AA0038/ChkIsAdmg';
    var T1GetExcel = '/api/AA0038/Excel';

    var T1Rec = 0;
    var T1LastRec = null;
    var isAdmin = 'N';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var fid = 'M';
    if (Ext.getUrlParam('fid') != '')
        fid = Ext.getUrlParam('fid');

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
            params: { p0: fid },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        matclassQueryStore.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            matclassQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
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
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
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
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.MiMastMaintain', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: fid
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
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
                //hidden: true,
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
                    var titleType = '';
                    if (fid == 'M')
                        titleType = '衛材基本檔維護作業';
                    else if (fid == 'ED')
                        titleType = '能設基本檔維護作業';
                    else if (fid == 'CN')
                        titleType = '通信基本檔維護作業';
                    else if (fid == 'AR')
                        titleType = '氣體基本檔維護作業';
                    var p = new Array();
                    p.push({ name: 'FN', value: titleType + '.xlsx' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: fid });
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
            text: "庫備識別碼",
            dataIndex: 'M_STOREID',
            width: 90
        }, {
            text: "合約識別碼",
            dataIndex: 'M_CONTID',
            width: 90
        }, {
            text: "ID碼",
            dataIndex: 'M_IDKEY',
            width: 60
        }, {
            text: "衛材料號碼",
            dataIndex: 'M_INVKEY',
            width: 80
        }, {
            text: "健保碼",
            dataIndex: 'M_NHIKEY',
            width: 100
        }, {
            text: "行政院碼",
            dataIndex: 'M_GOVKEY',
            width: 100
        }, {
            text: "長度(CM)",
            dataIndex: 'M_VOLL',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "寬度(CM)",
            dataIndex: 'M_VOLW',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "高度(CM)",
            dataIndex: 'M_VOLH',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "圓周",
            dataIndex: 'M_VOLC',
            align: 'right',
            style: 'text-align:left',
            width: 60
        }, {
            text: "材積轉換率",
            dataIndex: 'M_SWAP',
            align: 'right',
            style: 'text-align:left',
            width: 90
        }, {
            text: "是否聯標",
            dataIndex: 'M_MATID',
            width: 80
        }, {
            text: "聯標項次/院辦案號",
            dataIndex: 'E_ITEMARMYNO',
            width: 150
        }, {
            text: "合約年度",
            dataIndex: 'E_YRARMYNO',
            width: 100
        }, {
            text: "聯標組別",
            dataIndex: 'E_CLFARMYNO',
            width: 100
        }, {
            text: "是否供應契約",
            dataIndex: 'M_SUPPLYID',
            width: 90
        }, {
            text: "消耗屬性",
            dataIndex: 'M_CONSUMID',
            width: 80
        }, {
            text: "給付類別",
            dataIndex: 'M_PAYKIND',
            width: 80
        }, {
            text: "計費方式",
            dataIndex: 'M_PAYID',
            width: 80
        }, {
            text: "盤差種類",
            dataIndex: 'M_TRNID',
            width: 80
        }, {
            text: "申請申購識別碼",
            dataIndex: 'M_APPLYID',
            width: 100
        }, {
            text: "環保或衛署許可證",
            dataIndex: 'M_PHCTNCO',
            width: 120
        }, {
            text: "環保證號效期",
            dataIndex: 'M_ENVDT',
            width: 90
        }, {
            text: "廠商代碼",
            dataIndex: 'M_AGENNO',
            width: 80
        }, {
            text: "廠牌",
            dataIndex: 'M_AGENLAB',
            width: 80
        }, {
            text: "申購計量單位",
            dataIndex: 'M_PURUN',
            width: 90
        }, {
            text: "合約單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "折讓比",
            dataIndex: 'M_DISCPERC',
            align: 'right',
            style: 'text-align:left',
            width: 60
        }, {
            text: "批號效期註記",
            dataIndex: 'WEXP_ID',
            width: 100
        }, {
            text: "儲位記錄註記",
            dataIndex: 'WLOC_ID',
            width: 120
        }, {
            text: "是否作廢",
            dataIndex: 'CANCEL_ID',
            width: 80
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
            text: "最小撥補量",
            dataIndex: 'MIN_ORDQTY',
            align: 'right',
            style: 'text-align:left',
            width: 100
        }, {
            text: "廠商包裝轉換率",
            dataIndex: 'EXCH_RATIO',
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
            text: "來源代碼",
            dataIndex: 'E_SOURCECODE',
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
        var titleType = '';
        if (fid == 'M')
            titleType = '衛材';
        else if (fid == 'ED')
            titleType = '能設';
        else if (fid == 'CN')
            titleType = '通信';
        else if (fid == 'AR')
            titleType = '氣體';

        var strUrl = "AA0038_Form?strVtype=" + vType + "&strMmcode=" + strMmcode + "&strFid=" + fid + "&strIsadm=" + isAdmin;
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
            callableWin = GetPopWin(viewport, popform, titleType + '基本檔詳細資料', viewport.width - 20, viewport.height - 20);
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
    // 新增改成不限admin使用,但不可新增衛材
    //chkIsAdmin();

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    
});
